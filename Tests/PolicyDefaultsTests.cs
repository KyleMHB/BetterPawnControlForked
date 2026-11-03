using System.Collections.Generic;
using BetterPawnControlForked.CoreLogic;
using Xunit;

namespace BetterPawnControlForked.Core.Tests
{
    public class PolicyDefaultsTests
    {
        private sealed class Database
        {
            internal readonly object A = new object();
            internal readonly object B = new object();
            internal readonly List<object> Policies;
            internal int Writes;

            internal Database() { Policies = new List<object> { A, B }; }
            internal object Default() => Policies[0];
            internal void SetDefault(object policy)
            {
                int index = Policies.IndexOf(policy);
                object previous = Policies[0];
                Policies[0] = policy;
                Policies[index] = previous;
                Writes++;
            }
        }

        [Fact]
        public void VanillaChangeReplacesInitializedBpcDefault()
        {
            var db = new Database();
            object saved = db.A;
            db.SetDefault(db.B);
            Assert.Same(db.B, PolicyDefaults.Read(ref saved, db.Default));
            Assert.Same(db.B, saved);
            Assert.Equal(1, db.Writes);
        }

        [Fact]
        public void BpcChangeUpdatesVanillaForNewPawnDefaults()
        {
            var db = new Database();
            object saved = db.A;
            PolicyDefaults.Set(ref saved, db.B, db.Policies, db.SetDefault);
            Assert.Same(db.B, db.Default());
            Assert.Same(db.B, saved);
            Assert.Same(db.B, PolicyDefaults.Read(ref saved, db.Default));
            Assert.Equal(1, db.Writes);
        }

        [Fact]
        public void ReadingDefaultsDoesNotMakeBrowsedPolicyDefault()
        {
            var db = new Database();
            object saved = db.B;
            Assert.Same(db.A, PolicyDefaults.Read(ref saved, db.Default));
            Assert.Equal(0, db.Writes);
            Assert.Same(db.A, db.Default());
        }

        [Fact]
        public void DeletedCachedDefaultFallsBackToRemainingDatabasePolicy()
        {
            var db = new Database();
            object saved = db.A;
            db.Policies.Remove(db.A);
            Assert.Same(db.B, PolicyDefaults.Read(ref saved, db.Default));
            Assert.Same(db.B, saved);
            Assert.Equal(0, db.Writes);
        }

        [Fact]
        public void InvalidOrNullSelectionCannotCorruptVanillaDatabase()
        {
            var db = new Database();
            object saved = db.A;
            PolicyDefaults.Set(ref saved, null, db.Policies, db.SetDefault);
            PolicyDefaults.Set(ref saved, new object(), db.Policies, db.SetDefault);
            Assert.Same(db.A, saved);
            Assert.Same(db.A, db.Default());
            Assert.Equal(0, db.Writes);
        }

        [Fact]
        public void RestorePreservesLegacySavedBpcDefaultOverVanillaOrder()
        {
            var db = new Database();
            object saved = db.B;
            PolicyDefaults.Restore(ref saved, db.Policies, db.Default, db.SetDefault);
            Assert.Same(db.B, saved);
            Assert.Same(db.B, db.Default());
            // Capture and restore the current reference through another load.
            object reloaded = PolicyDefaults.Read(ref saved, db.Default);
            db.SetDefault(db.A);
            PolicyDefaults.Restore(ref reloaded, db.Policies, db.Default, db.SetDefault);
            Assert.Same(db.B, db.Default());
        }

        [Fact]
        public void RestoreMissingOrDeletedSavedReferenceUsesVanillaDefault()
        {
            var db = new Database();
            object saved = new object();
            PolicyDefaults.Restore(ref saved, db.Policies, db.Default, db.SetDefault);
            Assert.Same(db.A, saved);
            saved = null;
            PolicyDefaults.Restore(ref saved, db.Policies, db.Default, db.SetDefault);
            Assert.Same(db.A, saved);
            Assert.Equal(0, db.Writes);
        }

        [Fact]
        public void ColonistChangesPreserveSeparateSlaveAndPrisonerDefaults()
        {
            var db = new Database();
            object colonist = db.A;
            object slave = db.A;
            object prisoner = db.A;
            PolicyDefaults.Set(ref colonist, db.B, db.Policies, db.SetDefault);
            Assert.Same(db.A, PolicyDefaults.ReadSeparate(ref slave, db.Policies, db.Default));
            Assert.Same(db.A, PolicyDefaults.ReadSeparate(ref prisoner, db.Policies, db.Default));
            Assert.Same(db.B, db.Default());
        }

        [Fact]
        public void DeletedSeparateDefaultRepairsWithoutChangingColonistDefault()
        {
            var db = new Database();
            object slave = db.B;
            db.Policies.Remove(db.B);
            Assert.Same(db.A, PolicyDefaults.ReadSeparate(ref slave, db.Policies, db.Default));
            Assert.Same(db.A, slave);
            Assert.Same(db.A, db.Default());
            Assert.Equal(0, db.Writes);
        }
    }
}
