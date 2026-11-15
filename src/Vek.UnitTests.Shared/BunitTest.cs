// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Vek.UnitTests.Shared.Extensions;
using NUnit.Framework;

namespace Vek.UnitTests.Shared
{
    public abstract class BunitTest
    {
        protected Bunit.BunitContext Context { get; private set; } = null!;

        [SetUp]
        public virtual void Setup()
        {
            Context = new();
            Context.AddTestServices();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                Context.Dispose();
            }
            catch (Exception)
            {
                /*ignore*/
            }
        }

    }
}
