using System;
using System.IO;

namespace Osrs.Oncor.FileStore
{
    public sealed class FilestoreFile : Stream, IDisposable
    {
        internal static string TmpPath(Guid fileId)
        { return Path.Combine(FileStoreManager.basePath, "xx" + fileId.ToString("N")); }

        internal static string PermPath(Guid fileId)
        { return Path.Combine(FileStoreManager.basePath, fileId.ToString("N")); }

        private FileStream inner;

        public DateTime Created
        {
            get;
            private set;
        }

        public DateTime LastModified
        {
            get;
            private set;
        }

        public DateTime LastAccessed
        {
            get;
            private set;
        }

        public DateTime ExpiresAt
        {
            get;
            private set;
        }

        public bool IsTempFile
        {
            get;
            private set;
        }

        public Guid FileId
        {
            get;
        }

        public string FileName
        {
            get;
            set;
        }

        internal FilestoreFile(Guid fileId, DateTime created, DateTime lastMod, DateTime lastAcc, DateTime exp, bool isTmp, string filename)
        {
            this.FileId = fileId;
            this.Created = created;
            this.LastModified = lastMod;
            this.LastAccessed = lastAcc;
            this.ExpiresAt = exp;
            this.IsTempFile = isTmp;
            this.FileName = filename;
        }

        internal void Release()
        {
            try
            {
                if (this.inner != null)
                {
                    this.inner.Dispose();
                    this.inner = null;
                }
            }
            catch
            { }
        }

        internal bool ToPermanent()
        {
            if (this.IsTempFile)
            {
                lock (this)
                {
                    try
                    {
                        this.CleanClose();
                        File.Move(TmpPath(this.FileId), PermPath(this.FileId));
                        this.IsTempFile = false;
                        return true;
                    }
                    catch { }
                }
                return false;
            }
            return true; //already permanent
        }

        private void OpenFile()
        {
            lock (this) //maybe change this to use Interlocked?
            {
                if (this.inner == null)
                {
                    try
                    {
                        if (this.IsTempFile)
                            this.inner = File.Open(TmpPath(this.FileId), FileMode.Open, FileAccess.ReadWrite);
                        else
                            this.inner = File.Open(PermPath(this.FileId), FileMode.Open, FileAccess.ReadWrite);
                    }
                    catch { }
                }
            }
        }

        public override bool CanRead
        {
            get
            {
                this.OpenFile();
                if (this.inner != null)
                    return this.inner.CanRead;
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                this.OpenFile();
                if (this.inner != null)
                    return this.inner.CanSeek;
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                this.OpenFile();
                if (this.inner != null)
                    return this.inner.CanWrite;
                return false;
            }
        }

        public override long Length
        {
            get
            {
                this.OpenFile();
                if (this.inner != null)
                    return this.inner.Length;
                return -1;
            }
        }

        public override long Position
        {
            get
            {
                this.OpenFile();
                if (this.inner != null)
                    return this.inner.Position;
                return -1;
            }

            set
            {
                this.OpenFile();
                if (this.inner != null)
                    this.inner.Position=value;
            }
        }

        public override void Flush()
        {
            if (this.inner != null)
                this.inner.Flush();
        }

        private void CleanClose()
        {
            try
            {
                FileStream tmp = this.inner;
                this.inner = null;
                if (tmp!=null)
                    tmp.Dispose();
            }
            catch
            { }
        }

        public void Close()
        {
            lock (this)
            {
                if (this.inner != null)
                {
                    this.CleanClose();

                    IFileStoreProvider prov = FileStoreManager.Instance.GetProvider();
                    if (prov != null)
                        prov.Update(this);
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.OpenFile();
            if (this.inner != null)
                this.LastAccessed = DateTime.UtcNow;
            return this.inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.OpenFile();
            if (this.inner != null)
                this.LastAccessed = DateTime.UtcNow;
            return this.inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.OpenFile();
            if (this.inner != null)
            {
                this.LastAccessed = DateTime.UtcNow;
                this.LastModified = this.LastAccessed;
            }
            this.inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.OpenFile();
            if (this.inner != null)
            {
                this.LastAccessed = DateTime.UtcNow;
                this.LastModified = this.LastAccessed;
            }
            this.inner.Write(buffer, offset, count);
        }

        public new void Dispose()
        {
            try
            {
                Close();
            }
            catch
            { }
        }
    }
}
