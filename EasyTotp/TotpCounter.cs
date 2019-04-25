using System;

/**
* Counter whose value is a deterministic function of time as described in RFC 6238
* "TOTP: Time-Based One-Time Password Algorithm".
*
* <p>The 64-bit counter assumes the value {@code 0} at a predefined point in time and periodically
* increments its value by one periodically.
*
* <p>The value {@code V} of the counter at time instant {@code T} is:
* <pre>
* V = (T - T0) / TimeStep
* </pre>
* where {@code T0} is the earliest time instant at which the counter assumes the value {@code 0},
* and {@code TimeStep} is the duration of time for which the values of the counter remain constant.
*
* <p><em>Note: All time instants are in seconds since UNIX epoch, and all time durations are
* in seconds.</em>
*
* <p><em>Note: All time instants must be non-negative.</em>
*
* <p>Thread-safety: Instances of this class are immutable and are thus thread-safe.
*
* @author klyubin@google.com (Alex Klyubin)
*/
namespace EasyTotp
{
	class TotpCounter
	{

		/** Interval of time (seconds) between successive changes of this counter's value. */
		private readonly long mTimeStep;

		/**
	 * Earliest time instant (seconds since UNIX epoch) at which this counter assumes the value of
	 * {@code 0}.
	 */
		private readonly long mStartTime;

		/**
	 * Constructs a new {@code com.dreamsforge.TotpCounter} that starts with the value {@code 0} at time instant
	 * {@code 0} (seconds since UNIX epoch) and increments its value with the specified frequency.
	 *
	 * @param timeStep interval of time (seconds) between successive changes of this counter's value.
	 */
		public TotpCounter(long timeStep) : this(timeStep, 0) { }

		/**
	 * Constructs a new {@code com.dreamsforge.TotpCounter} that starts with the value {@code 0} at the specified
	 * time and increments its value with the specified frequency.
	 *
	 * @param timeStep interval of time (seconds) between successive changes of this counter's value.
	 * @param startTime the earliest time instant (seconds since UNIX epoch) at which this counter
	 *        assumes the value {@code 0}.
	 */
		private TotpCounter(long timeStep, long startTime)
		{
			if (timeStep < 1)
			{
				throw new /*IllegalArgumentException*/ ArgumentException("Time step must be positive: " + timeStep);
			}

			assertValidTime(startTime);

			mTimeStep = timeStep;
			mStartTime = startTime;
		}

		/**
	 * Gets the value of this counter at the specified time.
	 *
	 * @param time time instant (seconds since UNIX epoch) for which to obtain the value.
	 *
	 * @return value of the counter at the {@code time}.
	 */
		public long getValueAtTime(long time)
		{
			assertValidTime(time);

			// According to the RFC:
			// T = (Current Unix time - T0) / X, where the default floor function is used.
			//   T  - counter value,
			//   T0 - start time.
			//   X  - time step.

			// It's important to use a floor function instead of simple integer division. For example,
			// assuming a time step of 3:
			// Time since start time: -6 -5 -4 -3 -2 -1  0  1  2  3  4  5  6
			// Correct value:         -2 -2 -2 -1 -1 -1  0  0  0  1  1  1  2
			// Simple division / 3:   -2 -1 -1 -1  0  0  0  0  0  1  1  1  2
			//
			// To avoid using Math.floor which requires imprecise floating-point arithmetic, we
			// we compute the value using integer division, but using a different equation for
			// negative and non-negative time since start time.
			var timeSinceStartTime = time - mStartTime;
			if (timeSinceStartTime >= 0)
			{
				return timeSinceStartTime / mTimeStep;
			}

			return (timeSinceStartTime - (mTimeStep - 1)) / mTimeStep;
		}

		private static void assertValidTime(long time)
		{
			if (time < 0)
			{
				throw new ArgumentException("Negative time: " + time);
			}
		}
	}
}
