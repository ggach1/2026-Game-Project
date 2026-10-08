using System.Reflection;
using CIW.Code.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace _00.Work.CUH.Code.Gimmick.Editor
{
    public class SpringPadTests
    {
        private GameObject _root;
        private GameObject _springRoot;
        private Player _player;
        private Rigidbody2D _rigid;
        private SpringBounce2D _bounce;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [SetUp]
        public void SetUp()
        {
            _root = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/00.Work/CIW/02.Prefabs/Player.prefab"));
            _root.transform.position = new Vector3(0, 30, 0);
            _player = _root.GetComponent<Player>();
            typeof(Player).GetMethod("Awake", PrivateInstance).Invoke(_player, null);
            _rigid = _root.GetComponent<Rigidbody2D>();
            _springRoot = new GameObject("Spring bounce test");
            _bounce = _springRoot.AddComponent<SpringBounce2D>();
            var settings = new SerializedObject(_bounce);
            settings.FindProperty("playerInput").objectReferenceValue = _player.PlayerInput;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_springRoot);
            Object.DestroyImmediate(_root);
        }

        private void Step()
        {
            typeof(SpringBounce2D).GetMethod("FixedUpdate", PrivateInstance).Invoke(_bounce, null);
            typeof(PlayerMotor2D).GetMethod("FixedUpdate", PrivateInstance).Invoke(_player.Motor, null);
        }

        private void PressJump()
        {
            _player.Motor.RequestJump();
            typeof(SpringBounce2D).GetMethod("HandleJumpPressed", PrivateInstance).Invoke(_bounce, null);
        }

        [Test]
        public void BounceReplacesFallingSpeedAndPreservesHorizontalMovement()
        {
            _rigid.linearVelocity = new Vector2(7f, -20f);
            _player.Motor.SetMoveInput(1f);
            Assert.IsTrue(_bounce.TryBounce(_player, 16f, 24f, 0.16f));
            Assert.AreEqual(new Vector2(7f, 16f), _rigid.linearVelocity);
            Assert.AreEqual(1f, _player.Motor.MoveInput);
            Assert.IsNull(_player.Motor.CurrentPlatform);
            Assert.IsFalse(_player.GetModule<PlayerGroundSensor>().IsGrounded);
        }

        [Test]
        public void HeldJumpBoostsAndReleaseDoesNotCutSpringAscent()
        {
            PressJump();
            _bounce.TryBounce(_player, 16f, 24f, 0.16f);
            Assert.AreEqual(24f, _player.Motor.GetVerticalSpeed());
            _player.Motor.ReleaseJump();
            typeof(SpringBounce2D).GetMethod("HandleJumpReleased", PrivateInstance).Invoke(_bounce, null);
            Step();
            Assert.That(_player.Motor.GetVerticalSpeed(), Is.EqualTo(24f - 28f * Time.fixedDeltaTime).Within(0.001f));
        }

        [Test]
        public void LandingWindowAcceptsOneBoost()
        {
            _bounce.TryBounce(_player, 16f, 24f, 0.16f);
            Step();
            PressJump();
            Step();
            float boosted = _player.Motor.GetVerticalSpeed();
            Assert.That(boosted, Is.EqualTo(24f - 28f * Time.fixedDeltaTime).Within(0.001f));
            PressJump();
            Step();
            Assert.That(_player.Motor.GetVerticalSpeed(), Is.LessThan(boosted));
        }

        [Test]
        public void LatePressDoesNotBoost()
        {
            _bounce.TryBounce(_player, 16f, 24f, 0.04f);
            for (int i = 0; i < 4; i++) Step();
            float before = _player.Motor.GetVerticalSpeed();
            PressJump();
            Step();
            Assert.That(_player.Motor.GetVerticalSpeed(), Is.LessThan(before));
        }

        [Test]
        public void InvalidPowerAndDisabledSimulationRejectBounce()
        {
            Assert.IsFalse(_bounce.TryBounce(_player, float.NaN, 24f, 0.16f));
            Assert.IsFalse(_bounce.TryBounce(_player, 16f, 10f, 0.16f));
            _player.Motor.SetSimulationEnabled(false);
            Assert.IsFalse(_bounce.TryBounce(_player, 16f, 24f, 0.16f));
        }

        [Test]
        public void EndingBounceRestoresNormalJumpCut()
        {
            _bounce.TryBounce(_player, 16f, 24f, 0.16f);
            _rigid.linearVelocity = Vector2.down;
            Step();
            _rigid.linearVelocity = Vector2.up * 12f;
            _player.Motor.ReleaseJump();
            Step();
            Assert.That(_player.Motor.GetVerticalSpeed(), Is.EqualTo(12f * 0.45f - 28f * Time.fixedDeltaTime).Within(0.001f));
        }

        [Test]
        public void DisablingSpringClearsPendingInput()
        {
            PressJump();
            typeof(SpringBounce2D).GetMethod("OnDisable", PrivateInstance).Invoke(_bounce, null);
            _bounce.TryBounce(_player, 16f, 24f, 0.16f);
            Assert.AreEqual(16f, _player.Motor.GetVerticalSpeed());
        }
    }
}
