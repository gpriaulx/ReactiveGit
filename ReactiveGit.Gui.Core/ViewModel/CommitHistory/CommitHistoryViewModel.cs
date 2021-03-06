﻿// <copyright file="CommitHistoryViewModel.cs" company="Glenn Watson">
// Copyright (c) 2018 Glenn Watson. All rights reserved.
// See LICENSE file in the project root for full license information.
// </copyright>

using DynamicData.Binding;

namespace ReactiveGit.Gui.Core.ViewModel.CommitHistory
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using ReactiveGit.Core.Model;
    using ReactiveGit.Gui.Core.ExtensionMethods;
    using ReactiveGit.Gui.Core.Model;
    using ReactiveGit.Gui.Core.ViewModel.GitObject;

    using ReactiveUI;

    /// <summary>
    /// A view model related to a repository.
    /// </summary>
    public class CommitHistoryViewModel : GitObjectViewModelBase, ICommitHistoryViewModel
    {
        private readonly ObservableCollectionExtended<CommitHistoryItemViewModel> commitHistory =
            new ObservableCollectionExtended<CommitHistoryItemViewModel>();

        private readonly ReactiveCommand<Unit, GitCommit> refresh;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitHistoryViewModel" /> class.
        /// </summary>
        /// <param name="repositoryDetails">The details of the repository.</param>
        /// <exception cref="ArgumentException">If the repository does not exist.</exception>
        public CommitHistoryViewModel(IRepositoryDetails repositoryDetails)
        {
            if (repositoryDetails == null)
            {
                throw new ArgumentNullException(nameof(repositoryDetails));
            }

            this.RepositoryDetails = repositoryDetails;
            IObservable<bool> isCurrentBranchObservable =
                this.WhenAnyValue(x => x.RepositoryDetails.SelectedBranch).Select(x => x != null);

            this.refresh =
                ReactiveCommand.CreateFromObservable(
                    () => this.GetCommitsImpl(this.RepositoryDetails.SelectedBranch),
                    isCurrentBranchObservable);
            this.refresh.Subscribe(x => this.commitHistory.Add(new CommitHistoryItemViewModel(x)));

            isCurrentBranchObservable.Subscribe(x => this.Refresh.InvokeCommand());
        }

        /// <inheritdoc />
        public IEnumerable<CommitHistoryItemViewModel> CommitHistory => this.commitHistory;

        /// <inheritdoc />
        public override string FriendlyName => "Commit History";

        /// <summary>
        /// Gets a command that will refresh the display.
        /// </summary>
        public override ICommand Refresh => this.refresh;

        private IObservable<GitCommit> GetCommitsImpl(GitBranch branch)
        {
            this.commitHistory.Clear();
            return this.RepositoryDetails.BranchManager.GetCommitsForBranch(branch, 0, 0, GitLogOptions.IncludeMerges);
        }
    }
}