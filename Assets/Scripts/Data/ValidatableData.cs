using System;
using System.Text;

namespace BanSee.RoundedRectangleGenerator
{
    /// <summary>
    /// Base abstract class for all data classes that conform to the validation
    /// and can provide a validation error message explaining why the validation failed.
    /// </summary>
    [Serializable]
    public abstract class ValidatableData
    {
        private StringBuilder _validationErrorMessage = new StringBuilder();

        /// <summary>
        /// Property containing error message explaining why the data isn't valid.
        /// </summary>
        public string ValidationErrorMessage { get { return _validationErrorMessage.ToString(); } }

        /// <summary>
        /// Function checks the data and fills in the <see cref="_validationErrorMessage"/>
        /// with errors on what is incorrect in order for data to be valid, by calling the
        /// <see cref="AppendErrorMessage(string)"/> function.
        /// </summary>
        protected abstract void ValidateData();

        /// <summary>
        /// Function checks if the data is valid and fills in the <see cref="ValidationErrorMessage"/>
        /// containing the reason of the validation failure, if the validation fails.
        /// </summary>
        /// <returns>True if the data is valid, false otherwise.</returns>
        public virtual bool IsDataValid()
        {
            ClearErrorMessage();
            ValidateData();
            return string.IsNullOrEmpty(ValidationErrorMessage);
        }

        protected virtual void AppendErrorMessage(string errorMessage)
        {
            _validationErrorMessage.Append(errorMessage);
        }

        protected virtual void ClearErrorMessage()
        {
            _validationErrorMessage.Clear();
        }
    }
}
