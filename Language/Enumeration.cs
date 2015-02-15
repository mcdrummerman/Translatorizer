using System.Diagnostics;

namespace TranslatorizerConsole.Language
{    
    /// <summary>
    /// The enumeration. For parameters of Google Translation.
    /// </summary>
    public abstract class Enumeration
    {
        private readonly string name;

        private readonly string value;

        private readonly bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumeration"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        protected Enumeration(string value)
            : this(value, value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumeration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        protected Enumeration(string name, string value)
            : this(name, value, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumeration"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <param name="isDefault">if set to <c>true</c> it is default value.</param>
        protected Enumeration(string name, string value, bool isDefault)
        {
            this.name = name;
            this.isDefault = isDefault;
            this.value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is default.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is default; otherwise, <c>false</c>.
        /// </value>
        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get
            {
                return this.value;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Enumeration"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="enumeration">The enumeration.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(Enumeration enumeration)
        {
            if (enumeration.IsDefault)
            {
                return null;
            }

            return enumeration.Value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            if (!this.GetType().IsInstanceOfType(obj))
            {
                return false;
            }

            return this.Value == ((Enumeration)obj).Value;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            Debug.Assert(this.Value != null, "Value cannot be null.");

            return this.Value.GetHashCode();
        }
    }
}
