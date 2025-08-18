using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EventApi.ExeptionHandling.Exceptions
{
    public abstract class ImageGenerationServiceException : Exception
    {
        protected ImageGenerationServiceException(string message) : base(message) { }
    }

    public class InvalidEventDataException : ImageGenerationServiceException
    {
        public InvalidEventDataException(string message) : base(message) { }
    }
    public class MisconfiguredTemplateException : ImageGenerationServiceException
    {
        public MisconfiguredTemplateException(string message) : base(message) { }
    }
}