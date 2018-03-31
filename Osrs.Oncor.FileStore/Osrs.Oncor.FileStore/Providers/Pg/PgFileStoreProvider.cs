using System;
using System.Collections.Generic;
using System.IO;

namespace Osrs.Oncor.FileStore.Providers.Pg
{
    public sealed class PgFileStoreProvider : IFileStoreProvider
    {
        public bool Delete(FilestoreFile file)
        {
            if (file != null)
            {
                file.Close();
                return Delete(file.FileId, file.IsTempFile);
            }
            return false;
        }

        public bool Delete(Guid fileId)
        {
            FilestoreFile tmp = this.Get(fileId);
            if (tmp!=null)
                return Delete(tmp.FileId, tmp.IsTempFile);
            return false;
        }

        private bool Delete(Guid fileId, bool isTmp)
        {
            if (Db.Delete(fileId))
            {
                try
                {
                    if (isTmp && File.Exists(FilestoreFile.TmpPath(fileId)))
                        File.Delete(FilestoreFile.TmpPath(fileId));
                    else if (File.Exists(FilestoreFile.PermPath(fileId)))
                        File.Delete(FilestoreFile.PermPath(fileId));
                }
                catch { }
                return true;
            }
            return false;
        }

        public bool Exists(Guid fileId)
        {
            return Get(fileId) != null;
        }

        /// <summary>
        /// Replaces contents of permFile with contents of tmpFile.
        /// </summary>
        /// <param name="tmpFile">file to copy from</param>
        /// <param name="permFile">file to copy to</param>
        /// <returns></returns>
        public bool Replace(FilestoreFile tmpFile, FilestoreFile permFile)
        {
            if (tmpFile!=null && permFile!=null && !permFile.IsTempFile)
            {
                try
                {
                    permFile.Release();
                    tmpFile.Release();
                    File.Delete(FilestoreFile.PermPath(permFile.FileId));
                    if (tmpFile.IsTempFile)
                        File.Copy(FilestoreFile.TmpPath(tmpFile.FileId), FilestoreFile.PermPath(permFile.FileId));
                    else
                        File.Copy(FilestoreFile.PermPath(tmpFile.FileId), FilestoreFile.PermPath(permFile.FileId));
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public FilestoreFile Get(Guid fileId)
        {
            return Db.Open(fileId);
        }

        public FilestoreFile Make()
        {
            return Make(Guid.NewGuid());
        }

        public FilestoreFile Make(Guid fileId)
        {
            try
            {
                string tmpPath = FilestoreFile.TmpPath(fileId);
                string permPath = FilestoreFile.PermPath(fileId);

                if (!File.Exists(tmpPath) && !File.Exists(permPath))
                {
                    DateTime init = DateTime.UtcNow;
                    FileStream fs = File.Create(permPath);
                    fs.Dispose();
                    if (Db.Insert(fileId, init, init, init, DateTime.MaxValue, false, null))
                        return Db.Open(fileId);
                    else
                        File.Delete(permPath); //as a clean up
                }
            }
            catch { }

            return null;
        }

        public bool MakePermanent(FilestoreFile file)
        {
            if (file!=null)
            {
                if (file.IsTempFile)
                {
                    if (file.ToPermanent())
                        return this.Update(file);
                }
                else
                    return true; //already permanent
            }
            return false;
        }

        public bool MakePermanent(Guid fileId)
        {
            FilestoreFile tmp = Get(fileId);
            if (tmp != null)
                return MakePermanent(tmp);
            return false;
        }

        public FilestoreFile MakeTemp()
        {
            return MakeTemp(DateTime.UtcNow.AddDays(7)); //note we make temp files valid for 7 days
        }

        public FilestoreFile MakeTemp(DateTime expiration)
        {
            try
            {
                Guid id = Guid.NewGuid();
                string tmpPath = FilestoreFile.TmpPath(id);
                string permPath = FilestoreFile.PermPath(id);

                if (!File.Exists(tmpPath) && !File.Exists(permPath))
                {
                    DateTime init = DateTime.UtcNow;
                    FileStream fs = File.Create(tmpPath);
                    fs.Dispose();
                    if (Db.Insert(id, init, init, init, expiration, true, null))
                        return Db.Open(id);
                    else
                        File.Delete(tmpPath); //as a clean up
                }
            }
            catch { }

            return null;
        }

        public bool Update(FilestoreFile file)
        {
            if (file!=null)
                return Db.Update(file.FileId, file.LastModified, file.LastAccessed, file.ExpiresAt, file.IsTempFile, file.FileName);
            return false;
        }

        public void DeleteExpired()
        {
            DateTime cur = DateTime.UtcNow;
            cur = new DateTime(cur.Ticks, DateTimeKind.Utc);
            List<FilestoreFile> items = Db.GetExpired(cur);
            Db.DeleteExpired(items); //deletes the db records so nobody will come looking - makes it somewhat thread safe (unless currently locked) - all in one sql statement, which could fail if we have a ton of items (>2000 ish)

            foreach (FilestoreFile item in items)
            {
                try
                {
                    if (item.IsTempFile && File.Exists(FilestoreFile.TmpPath(item.FileId)))
                        File.Delete(FilestoreFile.TmpPath(item.FileId));
                    else if (File.Exists(FilestoreFile.PermPath(item.FileId)))
                        File.Delete(FilestoreFile.PermPath(item.FileId));
                }
                catch { }
            }
        }
    }
}
