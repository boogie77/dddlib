﻿// <copyright file="ValueObject.cs" company="dddlib contributors">
//  Copyright (c) dddlib contributors. All rights reserved.
// </copyright>

namespace dddlib
{
    /*  TODO (Cameron): 
        Fix exceptions (as before).
        Add expression equality based on public property values.
        Think about how this should work with the TypeDescriptors.  */

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using dddlib.Runtime;
    using dddlib.Sdk.Configuration.Model;

    /// <summary>
    /// Represents a value object.
    /// </summary>
    /// <typeparam name="T">The type of value object.</typeparam>
    //// LINK (Cameron): http://lostechies.com/jimmybogard/2007/06/25/generic-value-object-equality/
    public abstract class ValueObject<T> : IEquatable<T>
        where T : ValueObject<T>
    {
        private readonly IEqualityComparer<T> equalityComparer;
        private readonly T valueObject;

        internal ValueObject(IEqualityComparer<T> equalityComparer)
            : this(@this => new Config<T> { EqualityComparer = equalityComparer })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueObject{T}"/> class.
        /// </summary>
        protected ValueObject()
            : this(@this => Config<T>.From(Application.Current.GetValueObjectType(@this.GetType())))
        {
        }

        // LINK (Cameron): http://stackoverflow.com/questions/2287636/pass-current-object-type-into-base-constructor-call
        private ValueObject(Func<ValueObject<T>, Config<T>> configureValueObject)
        {
            var configuration = configureValueObject(this);

            this.equalityComparer = configuration.EqualityComparer;
            this.valueObject = (T)this; // NOTE (Cameron): Micro-optimization.
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not visible anywhere.")]
        public static bool operator ==(ValueObject<T> first, ValueObject<T> second)
        {
            return object.Equals(first, second);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not visible anywhere.")]
        public static bool operator !=(ValueObject<T> first, ValueObject<T> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>Returns <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public sealed override bool Equals(object obj)
        {
            return this.Equals(obj as T);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public sealed override int GetHashCode()
        {
            return this.equalityComparer.GetHashCode(this.valueObject);
        }

        /// <summary>
        /// Determines whether the specified object is equal to this instance.
        /// </summary>
        /// <param name="other">The object to compare with this instance.</param>
        /// <returns>Returns <c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
        //// LINK (Cameron): http://www.infoq.com/articles/Equality-Overloading-DotNET
        public virtual bool Equals(T other)
        {
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            if (object.ReferenceEquals(other, this))
            {
                return true;
            }

            if (other.GetType() != this.GetType())
            {
                // NOTE (Cameron): Type mismatch.
                return false;
            }

            return this.equalityComparer.Equals(this.valueObject, other);
        }

        private class Config<T1>
        {
            public IEqualityComparer<T1> EqualityComparer { get; set; }

            public static Config<T1> From(ValueObjectType valueObjectType)
            {
                return new Config<T1> { EqualityComparer = (IEqualityComparer<T1>)valueObjectType.EqualityComparer };
            }
        }
    }
}
