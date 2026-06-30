namespace SWeetJelly.Tween.Tests.EditMode
{
    using System;
    using NUnit.Framework;
    using SWeetJelly.Tween.Contract;
    using SWeetJelly.Tween.Domain;

    public sealed class TweenDomainTest
    {
        [Test]
        public void EasingMath_Clamp01_ClampsOutsideRange()
        {
            Assert.That(EasingMath.Clamp01(-1f), Is.EqualTo(0f));
            Assert.That(EasingMath.Clamp01(0.5f), Is.EqualTo(0.5f));
            Assert.That(EasingMath.Clamp01(2f), Is.EqualTo(1f));
        }

        [Test]
        public void EasingFunctionFactory_WithLinear_ClampsAndReturnsInput()
        {
            var easing = EasingFunctionFactory.Create(EaseType.Linear);

            Assert.That(easing.Evaluate(-1f), Is.EqualTo(0f));
            Assert.That(easing.Evaluate(0.25f), Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(easing.Evaluate(2f), Is.EqualTo(1f));
        }

        [Test]
        public void EasingFunctionFactory_WithQuadIn_ReturnsSquaredProgress()
        {
            var easing = EasingFunctionFactory.Create(EaseType.QuadIn);

            Assert.That(easing.Evaluate(0.5f), Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void CustomEasingFunction_ClampsInputBeforeEvaluate()
        {
            var lastInput = -1f;
            var easing = new CustomEasingFunction(t =>
            {
                lastInput = t;
                return t;
            });

            Assert.That(easing.Evaluate(2f), Is.EqualTo(1f));
            Assert.That(lastInput, Is.EqualTo(1f));
        }

        [Test]
        public void CustomEasingFunction_WithNullEvaluator_ThrowsArgumentNullException()
        {
            var exception = AssertThrows<ArgumentNullException>(() =>
                new CustomEasingFunction(null));

            Assert.That(exception.ParamName, Is.EqualTo("evaluator"));
        }

        private static TException AssertThrows<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException exception)
            {
                return exception;
            }

            Assert.Fail($"Expected exception of type {typeof(TException).Name}.");
            return null;
        }
    }
}
