﻿// ***********************************************************************
// Assembly         : TessellationAndVoxelizationGeometryLibrary
// Author           : Design Engineering Lab
// Created          : 06-05-2015
//
// Last Modified By : Matt
// Last Modified On : 05-26-2016
// ***********************************************************************
// <copyright file="Point2D.cs" company="Design Engineering Lab">
//     Copyright ©  2014
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MIConvexHull;
using StarMathLib;

namespace TVGL
{
    /// <summary>
    ///     The Point light struct is a low memory version of the Point class. 
    /// </summary>
    [DataContract]
    public struct PointLight: IVertex
    {
        [DataMember]
        public double[] Position { get; set; }

        public double X => Position[0];

        public double Y => Position[1];

        public PointLight(double x, double y)
        {
            Position = new []{x , y};
        }

        public PointLight(Point point)
        {
            Position = new[] { point.X, point.Y };
        }

        public PointLight(double[] position)
        {
            Position = new[] { position[0], position[1] };
        }
    }

    /// <summary>
    ///     The Point class is used to indicate a 2D or 3D location that may be outside
    ///     of a solid (hence making Vertex an inappropriate choice).
    ///     One of the useful aspects of the point object is that they contain a
    ///     reference (or References) to vertices that may be representing in a
    ///     transformed way. For example "Get2DProjection" returns the 2D projection of
    ///     a set of vertices without changing those vertices. This is done by "wrapping"
    ///     these Point objects around a vertex and then providing their new position.
    /// </summary>
    [DataContract]
    public class Point : IVertex
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>The x.</value>
        public double X => Light.X;

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>The y.</value>
        public double Y => Light.Y;

        /// <summary>
        /// Gets or sets the z coordinate, which is mostly ignored. 
        /// It is initialized as 0.0 and accessed here or with Position3D
        /// </summary>
        /// <value>The z.</value>
        [DataMember]
        public double Z { get; set; } = 0.0;

        /// <summary>
        ///     Gets or sets the references.
        /// </summary>
        /// <value>The references.</value>
        /// Cannot serialize vertices yet. Not a circular reference problem.
        public List<Vertex> References { get; set; }

        /// <summary>
        ///     Gets or sets the index in a path
        /// </summary>
        [DataMember]
        public int IndexInPath { get; set; }

        /// <summary>
        ///  Gets or sets the index of the polygon that this point belongs to
        /// </summary>
        [DataMember]
        public int PolygonIndex { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// Cannot serialize lines yet. Not sure if circular reference will cause issues.
        public IList<Line> Lines { get; set; }

        /// <summary>
        ///     Gets the coordinates or position as an array of 3 elements (X, Y, Z).
        /// </summary>
        /// <value>The coordinates or position.</value>
        public double[] Position3D => new[] { X, Y, Z };

        /// <summary>
        ///     Gets or sets the coordinates or position as an array of 2 elements (X, Y).
        /// </summary>
        /// <value>The coordinates or position.</value>
        [DataMember]
        public double[] Position => Light.Position;
        
        [DataMember]
        public PointLight Light { get; set; }

        /// <summary>
        ///     Gets or sets an arbitrary ReferenceIndex to track point
        /// </summary>
        /// <value>The reference index.</value>
        public int ReferenceIndex { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="v">The v.</param>
        public Point(Vertex v)
            : this(v, v.Position[0], v.Position[1], 0.0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(Vertex vertex, double x, double y)
            : this(vertex, x, y, 0.0)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TVGL.Point" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public Point(double x, double y)
            : this(null, x, y, 0.0)
        {
            if(double.IsNaN(x) || double.IsNaN(y)) throw new Exception("Must be a number");
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TVGL.Point" /> class.
        /// </summary>
        public Point(PointLight p)
            : this(null, p.X, p.Y, 0.0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="point"></param>
        public Point(Point point)
        {
            Light = new PointLight(point.X, point.Y);
            Z = point.Z;
            Lines = new List<Line>(point.Lines);
            References = new List<Vertex>(point.References);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public Point(Vertex vertex, double x, double y, double z)
        {
            Light = new PointLight(x, y);
            Z = z;
            Lines = new List<Line>();
            References = new List<Vertex>();
            if (vertex == null) return;
            References.Add(vertex);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Point" /> class.
        /// </summary>
        /// <param name="coordinates">The coordinates.</param>
        public Point(IList<double> coordinates) : this(null, coordinates[0], coordinates[1])
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     this point
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>System.Double.</returns>
        public double this[int index] => Position[index];

        /// <summary>
        /// Gets whether points are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Point a, Point b)
        {
            //First, check the references. This is very fast.
            if (ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.X.IsPracticallySame(b.X) && a.Y.IsPracticallySame(b.Y);
        }

        public Point Copy()
        {
            return new Point(X, Y);
        }

        /// <summary>
        /// Gets whether points are not equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Checks if this intPoint is equal to the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Point)) return false;
            var a = (Point)obj;
            return ReferenceEquals(a, this);
        }

        /// <summary>
        /// Gets the HashCode for this Point
        /// </summary>
        /// <returns></returns>
        public sealed override int GetHashCode()
        {
            unchecked
            {
                //Using prime numbers to get unique hashcodes
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                return hashCode;
            }
        }

        internal bool InResult;
        internal bool InResultMultipleTimes;

        #endregion
    }
}