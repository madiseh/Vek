// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using Vek.Charts;


namespace Vek
{
    /// <summary>
    /// Indicates the technique used to smooth lines connecting values in a <see cref="VekAxisLineChartBase{T, TOptions}"/>.
    /// </summary>
    public enum InterpolationOption
    {
        /// <summary>
        /// Lines are smoothed to pass through each data point as a natural spline.
        /// </summary>
        NaturalSpline,

        /// <summary>
        /// Lines are smoothed to pass through each data point as an end-point spline.
        /// </summary>
        EndSlope,

        /// <summary>
        /// Lines are smoothed to pass through each data point as a periodic spline.
        /// </summary>
        Periodic,

        /// <summary>
        /// Lines are straight from one point to the next.
        /// </summary>
        Straight
    }
}
