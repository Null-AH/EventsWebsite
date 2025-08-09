using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventApi.DTO.Template;
using EventApi.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EventApi.Mappers
{
    public static class TemplateMapper
    {
        public static List<TemplateElement> ToTemplateElementsFromTemplateDto(this List<TemplateElementsDto> templateElementsDto, int eventId)
        {
            return templateElementsDto.Select(dto => new TemplateElement
            {
                ElementType = dto.ElementType,
                X = dto.X,
                Y = dto.Y,
                Width = dto.Width,
                Height = dto.Height,
                FontColor = dto.FontColor,
                FontTheme = dto.FontTheme,
                EventId = eventId
            }).ToList();
        }
        public static List<TemplateElementsDto> TemplateElementsToDto(this List<TemplateElement> templateElementsModel)
        {
            return templateElementsModel.Select(temp => new TemplateElementsDto
            {
                ElementType = temp.ElementType,
                X = temp.X,
                Y = temp.Y,
                Width = temp.Width,
                Height = temp.Height,
                FontColor = temp.FontColor,
                FontTheme = temp.FontTheme,
            }).ToList();
        }
        
    }
}