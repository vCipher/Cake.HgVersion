﻿using System;
using System.Collections.Generic;
using System.Linq;
using HgVersion.VCS;
using Mercurial;

namespace Cake.HgVersion.VCS
{
    /// <inheritdoc />
    public sealed class HgRepository : IRepository
    {
        private readonly Repository _repository;

        /// <summary>
        /// Creates an instance of <see cref="HgRepository"/>
        /// </summary>
        /// <param name="repository">Mercurial.Net <see cref="Repository"/></param>
        public HgRepository(Repository repository)
        {
            _repository = repository;
        }

        /// <inheritdoc />
        public string Path => _repository.Path;
        
        /// <inheritdoc />
        public IEnumerable<ICommit> Log()
        {
            return _repository
                .Log()
                .Select(changeset => (HgCommit) changeset);
        }

        /// <inheritdoc />
        public IEnumerable<ICommit> Log(ILogQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            
            if (!(query is HgLogQuery hgQuery))
                throw new InvalidOperationException($"{query.GetType()} is not supported.");

            return _repository
                .Log(hgQuery.Revision)
                .Select(changeset => (HgCommit) changeset);
        }

        /// <inheritdoc />
        public IEnumerable<ICommit> Log(Func<ILogQueryBuilder, ILogQuery> config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            
            var builder = new HgLogQueryBuilder();
            var query = config(builder);

            return Log(query);
        }

        /// <inheritdoc />
        public IEnumerable<ICommit> Heads()
        {
            return _repository
                .Heads()
                .Select(changeset => (HgCommit) changeset);
        }

        /// <inheritdoc />
        public string Branch()
        {
            return _repository.Branch();
        }

        /// <inheritdoc />
        public string Branch(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            
            return _repository.Branch(name);
        }

        /// <inheritdoc />
        public IEnumerable<IBranchHead> Branches()
        {
            return _repository
                .Branches()
                .Select(head => new HgBranchHead(
                    head.Name, 
                    () => GetCommit(head.RevisionNumber)));
        }

        /// <inheritdoc />
        public ICommit Tip()
        {
            return (HgCommit) _repository.Tip();
        }

        /// <inheritdoc />
        public void Tag(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            
            _repository.Tag(name);
        }

        /// <inheritdoc />
        public IEnumerable<ITag> Tags()
        {
            return _repository
                .Tags()
                .Select(tag => new HgTag(
                    tag.Name,
                    () => GetCommit(tag.RevisionNumber)));
        }

        /// <inheritdoc />
        public void AddRemove()
        {
            _repository.AddRemove();
        }

        /// <inheritdoc />
        public string Commit(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            return _repository.Commit(message);
        }

        /// <summary>
        /// Converts a <see cref="Repository"/> into a <see cref="HgRepository"/>
        /// </summary>
        /// <param name="repository">Mercurial.Net <see cref="Repository"/></param>
        /// <returns></returns>
        public static implicit operator HgRepository(Repository repository) =>
            new HgRepository(repository);

        
        private ICommit GetCommit(int revisionNumber)
        {
            var id = _repository.Identify(new IdentifyCommand()
                .WithAdditionalArgument($"--rev {revisionNumber}"));

            var log = _repository.Log(new LogCommand()
                .WithRevision(id)
                .WithAdditionalArgument("--limit 1"));

            return (HgCommit)log.First();
        }
    }
}