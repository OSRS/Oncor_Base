using System;

namespace Osrs.Oncor.FileStore
{
    public interface IFileStoreProvider
    {
        bool Exists(Guid fileId);

        FilestoreFile Get(Guid fileId);

        bool Replace(FilestoreFile tmpFile, FilestoreFile permFile);

        FilestoreFile MakeTemp();
        FilestoreFile MakeTemp(DateTime expiration);

        FilestoreFile Make(Guid fileId);

        FilestoreFile Make();

        bool Update(FilestoreFile file);

        bool MakePermanent(Guid fileId);

        bool MakePermanent(FilestoreFile file);


        bool Delete(Guid fileId);

        bool Delete(FilestoreFile file);

        void DeleteExpired();
    }
}
