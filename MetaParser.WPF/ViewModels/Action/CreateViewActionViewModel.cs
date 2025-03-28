﻿using CommunityToolkit.Mvvm.Input;
using MetaParser.Models;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace MetaParser.WPF.ViewModels;

public partial class CreateViewActionViewModel : ActionViewModel
{
    private class EncodingStringWriter : StringWriter
    {
        public EncodingStringWriter(Encoding enc) => Encoding = enc;

        public override Encoding Encoding { get; }
    }

    public CreateViewActionViewModel(CreateViewMetaAction action, MetaViewModel meta) : base(action, meta)
    { }

    public string ViewName
    {
        get => ((CreateViewMetaAction)Action).ViewName;
        set
        {
            if (((CreateViewMetaAction)Action).ViewName != value)
            {
                ((CreateViewMetaAction)Action).ViewName = value;
                OnPropertyChanged(nameof(ViewName));
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
            }
        }
    }

    public string ViewDefinition
    {
        get => ((CreateViewMetaAction)Action).ViewDefinition;
        set
        {
            if (((CreateViewMetaAction)Action).ViewDefinition != value)
            {
                ((CreateViewMetaAction)Action).ViewDefinition = value;
                OnPropertyChanged(nameof(ViewDefinition));
                OnPropertyChanged(nameof(Display));
                IsDirty = true;
            }
        }
    }

    [RelayCommand]
    void MinifyXML()
    {
        var doc = LoadXML();

        if (doc != null)
            ViewDefinition = doc.OuterXml;
    }

    [RelayCommand]
    void PrettifyXML()
    {
        var doc = LoadXML();

        if (doc != null)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace
            };

            using var writer = new EncodingStringWriter(null);
            using var writer2 = XmlWriter.Create(writer, settings);
            doc.Save(writer2);

            ViewDefinition = writer.ToString();
        }
    }

    private XmlDocument LoadXML()
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(ViewDefinition);
            using var ms = new MemoryStream(bytes);
            ms.Flush();
            ms.Position = 0;

            var doc = new XmlDocument();
            doc.PreserveWhitespace = false;
            doc.Load(ms);

            return doc;
        }
        catch
        {
            return null;
        }
    }
}
