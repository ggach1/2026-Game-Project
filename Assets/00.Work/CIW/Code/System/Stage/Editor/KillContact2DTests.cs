using CIW.Code.System.Interface;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CIW.Code.System.Stage.Editor
{
    public sealed class KillContactTestTarget : MonoBehaviour, IKillable, IKillableHitbox
    {
        public bool IsAlive { get; private set; } = true;
        public Collider2D Body;
        public int Deaths;
        public DeathContext LastDeath;
        public bool IsDeathHitbox(Collider2D collider) => collider == Body;
        public void Kill(DeathContext context) { IsAlive = false; Deaths++; LastDeath = context; }
    }

    public class KillContact2DTests
    {
        GameObject _root;
        KillContactTestTarget _target;
        Collider2D _body;
        readonly DeathContext _context = new DeathContext(Vector2.one, Vector2.left, DeathCause.Saw);

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Death target") { layer = 3 };
            _target = _root.AddComponent<KillContactTestTarget>();
            var child = new GameObject("Body");
            child.transform.SetParent(_root.transform);
            _body = child.AddComponent<BoxCollider2D>();
            _target.Body = _body;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [Test]
        public void ChildBodyUsesOwnerLayerAndPassesContextWithoutDisablingPlayer()
        {
            Assert.IsTrue(KillContact2D.TryKill(_body, 1 << 3, _context));
            Assert.AreEqual(1, _target.Deaths);
            Assert.AreEqual(DeathCause.Saw, _target.LastDeath.Cause);
            Assert.AreEqual(Vector2.one, _target.LastDeath.HitPoint);
            Assert.IsTrue(_root.activeSelf);
            Assert.IsTrue(_body.gameObject.activeSelf);
        }

        [Test]
        public void SensorContactDoesNotKill()
        {
            var sensor = _root.AddComponent<BoxCollider2D>();
            sensor.isTrigger = true;
            Assert.IsFalse(KillContact2D.TryKill(sensor, 1 << 3, _context));
            Assert.AreEqual(0, _target.Deaths);
        }

        [Test]
        public void DeadTargetDoesNotReceiveDuplicateDeath()
        {
            KillContact2D.TryKill(_body, 1 << 3, _context);
            Assert.IsFalse(KillContact2D.TryKill(_body, 1 << 3, _context));
            Assert.AreEqual(1, _target.Deaths);
        }

        [Test]
        public void WrongLayerDisabledColliderAndNullAreIgnored()
        {
            Assert.IsFalse(KillContact2D.TryKill(_body, 1 << 6, _context));
            _body.enabled = false;
            Assert.IsFalse(KillContact2D.TryKill(_body, 1 << 3, _context));
            Assert.IsFalse(KillContact2D.TryKill(null, ~0, _context));
            Assert.AreEqual(0, _target.Deaths);
        }

        [Test]
        public void ResetMotionMovesTransformAndBodyBeforeSimulationResumes()
        {
            var body = _root.AddComponent<Rigidbody2D>();
            var motor = _root.AddComponent<CIW.Code.Player.PlayerMotor2D>();
            var serialized = new SerializedObject(motor);
            serialized.FindProperty("rigid").objectReferenceValue = body;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _root.transform.position = new Vector3(4, -2, 3);
            Physics2D.SyncTransforms();
            motor.SetSimulationEnabled(false);

            var spawn = new Vector2(-6, 1);
            motor.ResetMotion(spawn);
            Assert.AreEqual(spawn, body.position);
            Assert.AreEqual(new Vector3(-6, 1, 3), _root.transform.position);
            Assert.AreEqual(Vector2.zero, body.linearVelocity);
            motor.SetSimulationEnabled(true);
            Assert.AreEqual(spawn, body.position);
            Assert.AreEqual(new Vector3(-6, 1, 3), _root.transform.position);
        }

        [TestCase("A1")]
        [TestCase("A2")]
        [TestCase("B1")]
        public void TestSectionHasConfiguredFallZone(string id)
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/00.Work/CIW/StageFlowTest/Section_{id}.prefab");
            Assert.IsNotNull(root.GetComponent<KillZone2D>());
            var collider = root.GetComponent<BoxCollider2D>();
            Assert.IsTrue(collider.isTrigger);
            Assert.AreEqual(-6f, collider.offset.y + collider.size.y / 2f);
            var zone = new SerializedObject(root.GetComponent<KillZone2D>());
            Assert.AreEqual((int)DeathCause.Fall, zone.FindProperty("cause").intValue);
        }
    }
}
