using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EventApi.Models
{
    [Table("TemplateElements")]
    public class TemplateElement
    {
        public int Id { get; set; }
        public string ElementType  { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? FontColor { get; set; }
        public string? FontTheme { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; }           
    }
}