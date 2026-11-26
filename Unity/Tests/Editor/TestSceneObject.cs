// Copyright © 2026 Miris, Inc. All rights reserved.

using Miris.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Miris.Tests.Editor
{
    public class TestSceneObject
    {
        // Composes the whole parent chain.
        const int NoAncestorId = -1;

        // Every non-constant element is distinct, so a misplaced element changes the result.
        static readonly Matrix4x4 Placement = new(
            new Vector4(1.5f, 0.25f, -0.5f, 0.0f),
            new Vector4(-0.75f, 2.0f, 0.125f, 0.0f),
            new Vector4(0.375f, -1.25f, 3.0f, 0.0f),
            new Vector4(4.0f, -5.0f, 6.0f, 1.0f));

        Context m_context;
        Client m_client;
        SceneObject m_root;

        [SetUp]
        public void SetUp()
        {
            m_context = new Context();
            m_client = new Client(m_context);
            m_root = new SceneObject(m_client, m_client.GetSceneRootObjectId());
            m_root.SetTransform(Placement);
        }

        [TearDown]
        public void TearDown()
        {
            m_client.Dispose();
            m_context.Dispose();
        }

        [Test]
        public void GetTransformReturnsTheSetTransform()
        {
            Assert.AreEqual(Placement, m_root.GetTransform());
        }

        [Test]
        public void GetWorldTransformOfTheRootIsItsTransform()
        {
            Assert.AreEqual(Placement, m_root.GetWorldTransform());
        }

        [Test]
        public void GetTransformRelativeToNoAncestorIsTheWholeChain()
        {
            Assert.AreEqual(Placement, m_root.GetTransformRelativeToAncestor(NoAncestorId));
        }
    }
}
