using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Oereb.Report.Helper;
using Oereb.Service.DataContracts;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace Oereb.Report
{
    /// <summary>
    /// mapping class, from xml to report
    /// </summary>
    
    [DataObject]
    public class ReportExtract
    {
        public Service.DataContracts.Model.v04.Extract Extract { get; set; }
        public string Language { get; set; } = "de";

        public TocRegion Toc { get; set; }
        public List<TocAppendix> TocAppendixes { get; set; }
        public int AppendixCounter { get; set; } = 1;

        public bool ExtractComplete { get; set; } = true;

        public string Title {
            get
            {
                var postfix = !ExtractComplete ? " mit reduzierter Information" : string.Empty;
                return $"Auszug aus dem Kataster der\nöffentlich-rechtlichen Eigentumsbeschränkungen \n(ÖREB - Kataster){postfix}";
            }
        }

        public bool AttacheFiles { get; set; } = false;

        public List<BodyItem> ReportBodyItems { get; set; }
        public List<GlossaryItem> GlossaryItems { get; set; }

        public Image ImageTitle { get; set; }
        public GeoreferenceExtension GeoreferenceExtensionTitle { get; set; }
        public ImageExtension ImageRestrictionOnLandownership { get; set; }
        public List<ImageExtension> AdditionalLayers { get; set; }

        public string PlrCadastreAuthority
        {
            get
            {
                var office = Extract.PLRCadastreAuthority;
                return $"{Helper.LocalisedText.GetStringFromArray(office.Name, Language)}, {office.Street} {office.Number}, {office.PostalCode} {office.City}";
            }
        }

        public int BodySectionCount => Extract.RealEstate.RestrictionOnLandownership.GroupBy(x => x.Theme.Code + "|" +x.SubTheme ?? "").ToList().Count;

        public int BodySectionFlag { get; set; } = -1;

        public ReportExtract()
        {
        }

        public ReportExtract(bool extractComplete, bool attacheFiles)
        {
            ExtractComplete = extractComplete;
            AttacheFiles = attacheFiles;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<BodyItem> GetBodyItems()
        {
            //path for the designer preview in visual studio
            //Extract = Xml<Service.DataContracts.Model.v04.Extract>.DeserializeFromFile(@"...");
            Ini();
            return ReportBodyItems;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public ReportExtract GetReportExtract()
        {
            //path for the designer preview in visual studio
            //Extract = Xml<Service.DataContracts.Model.v04.Extract>.DeserializeFromFile(@"...");
            Ini();
            return this;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<BodyItem> GetBodyItemsByExtract(ReportExtract reportExtract)
        {
            return reportExtract.ReportBodyItems;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public ReportExtract GetReportByExtract(ReportExtract reportExtract)
        {
            return reportExtract;
        }

        public void Ini()
        {
            AdditionalLayers = new List<ImageExtension>();

            //ini title image and extensions

            var titleExtention = Extract.RealEstate.PlanForLandRegister.extensions?.Any.FirstOrDefault(x => x.LocalName == "MapExtension");

            if (titleExtention != null)
            {
                var titleExtentionElement = XElement.Parse(titleExtention.OuterXml);

                var imageBg = PreProcessing.GetImageFromByteArray(Extract.RealEstate.PlanForLandRegister.Image);
                var image = new Bitmap(imageBg.Width, imageBg.Height, PixelFormat.Format32bppArgb);

                image = PreProcessing.MergeTwoImages(image, imageBg);

                var georeferenceExtention = GetGeoreferenceExtension(titleExtentionElement);

                var parcelHighligthed = Helper.Geometry.RasterizeGeometryFromGml(
                    Extract.RealEstate.Limit,
                    new double[] { georeferenceExtention.Extent.Xmin, georeferenceExtention.Extent.Ymin, georeferenceExtention.Extent.Xmax, georeferenceExtention.Extent.Ymax },
                    image.Width,
                    image.Height
                );

                image = PreProcessing.MergeTwoImages(image, parcelHighligthed);
                ImageTitle = image;
            }
            else
            {
                ImageTitle = PreProcessing.GetImageFromByteArray(Extract.RealEstate.PlanForLandRegister.Image);
            }

            var realEstateExtension = Extract.RealEstate.extensions?.Any.FirstOrDefault(x => x.LocalName == "RealEstateExtension");

            if (realEstateExtension != null)
            {
                var realEstateElement = XElement.Parse(realEstateExtension.OuterXml);

                ImageRestrictionOnLandownership = GetImageExtension(realEstateElement.Element("PlanForROL"));

                foreach (var additionalLayerElement in realEstateElement.Elements("AdditionalLayer"))
                {
                    var item = GetImageExtension(additionalLayerElement);

                    if (item == null)
                    {
                        continue;
                    }

                    AdditionalLayers.Add(item);
                }
            }
            else
            {
                ImageRestrictionOnLandownership = new ImageExtension() {Image = ImageTitle, Seq = 0, Transparency = 0};
            }

            IniReportBody();
            IniReportGlossary();
            IniToc(Extract, Language);
        }

        #region General

        public DescriptionExtension GetDescriptionExtension(XElement extension)
        {
            var descriptionExtension = new DescriptionExtension();

            descriptionExtension.Seq = extension.Element("Seq") == null ? 5 : System.Convert.ToInt32(extension.Element("Seq").Value);
            descriptionExtension.Transparency = extension.Element("Transparency") == null ? 0.5 : System.Convert.ToDouble(extension.Element("Transparency").Value);

            return descriptionExtension;
        }

        public GeoreferenceExtension GetGeoreferenceExtension(XElement extension)
        {
            if (extension.Element("Extent") == null ||
                extension.Element("Extent").Element("Xmin") == null || extension.Element("Extent").Element("Ymin") == null ||
                extension.Element("Extent").Element("Xmax") == null || extension.Element("Extent").Element("Ymax") == null)
            {
                return null;
            }

            var georeferenceExtension = new GeoreferenceExtension();

            georeferenceExtension.Extent = new Extent();
            georeferenceExtension.Extent.Xmin = System.Convert.ToDouble(extension.Element("Extent").Element("Xmin").Value);
            georeferenceExtension.Extent.Ymin = System.Convert.ToDouble(extension.Element("Extent").Element("Ymin").Value);
            georeferenceExtension.Extent.Xmax = System.Convert.ToDouble(extension.Element("Extent").Element("Xmax").Value);
            georeferenceExtension.Extent.Ymax = System.Convert.ToDouble(extension.Element("Extent").Element("Ymax").Value);
            georeferenceExtension.SetDescription(GetDescriptionExtension(extension));

            return georeferenceExtension;
        }

        public ImageExtension GetImageExtension(XElement extension)
        {
            if (extension.Element("Image") == null || extension.Element("Extent")== null || 
                extension.Element("Extent").Element("Xmin") == null || extension.Element("Extent").Element("Ymin") == null || 
                extension.Element("Extent").Element("Xmax") == null || extension.Element("Extent").Element("Ymax") == null)
            {
                return null;
            }

            var imageExtension = new ImageExtension();

            imageExtension.Image = Helper.HImage.Base64ToImage(extension.Element("Image").Value);
            imageExtension.SetGeoreference(GetGeoreferenceExtension(extension));
            imageExtension.Name = extension.Element("Topicname") == null ? "" : extension.Element("Topicname").Value;
            return imageExtension;
        }

        public static GeometryExtension GetGeometryExtension(XElement extension)
        {
            return new GeometryExtension
            {
                Type = extension.Element("Type") == null ? "Unknown" : extension.Element("Type").Value
            };
        }

        public class DescriptionExtension
        {
            public int Seq { get; set; }
            public double Transparency { get; set; }
        }

        public class GeoreferenceExtension : DescriptionExtension
        {
            public Extent Extent { get; set; }

            public void SetDescription(DescriptionExtension descriptionExtension)
            {
                Seq = descriptionExtension.Seq;
                Transparency = descriptionExtension.Transparency;
            }
        }

        public class ImageExtension : GeoreferenceExtension
        {
            public string Name { get; set; }
            public Image Image { get; set; }

            public void SetGeoreference(GeoreferenceExtension georeferenceExtension)
            {
                Extent = georeferenceExtension.Extent;
                Transparency = georeferenceExtension.Transparency;
                Seq = georeferenceExtension.Seq;
            }
        }

        public class GeometryExtension
        {
            public string Type { get; set; }
        }

        public class Extent
        {
            public double Xmin { get; set; }
            public double Ymin { get; set; }
            public double Xmax { get; set; }
            public double Ymax { get; set; }
        }

        #endregion

        #region TOC

        public void IniToc(Service.DataContracts.Model.v04.Extract extract, string language )
        {
            Toc = new TocRegion();
            TocAppendixes = new List<TocAppendix>();
            Toc.Extract = extract;
            Toc.Language = language;

            Toc.GeneralInformation = Helper.LocalisedMText.GetStringFromArray(Extract.GeneralInformation, Language);
            Toc.BaseData = Helper.LocalisedMText.GetStringFromArray(Extract.BaseData, Language);
            Toc.ExclusionOfLiabilityTitle = Extract.ExclusionOfLiability.Select(x => Helper.LocalisedText.GetStringFromArray(x.Title, Language)).First();
            Toc.ExclusionOfLiabilityContent = Extract.ExclusionOfLiability.Select(x => Helper.LocalisedMText.GetStringFromArray(x.Content, Language)).First();

            var groupedBodyItems = ReportBodyItems.GroupBy(x => x.Theme).ToList(); // Extract.RealEstate.RestrictionOnLandownership.GroupBy(x => x.Theme.Code).ToList();

            foreach (var bodyItem in groupedBodyItems)
            {
                var tocItem = new TocItem()
                {
                    Page = 0,
                    Label = bodyItem.First().Theme,
                    Appendixes = new List<TocAppendix>()
                };

                if (ExtractComplete)
                {
                    foreach (var item in bodyItem.ToList())
                    {
                        foreach (var legalProvision in item.LegalProvisions)
                        {
                            if (string.IsNullOrEmpty(legalProvision.Url))
                            {
                                continue; //an empty url is possible
                            }

                            var tocAppendix = new TocAppendix()
                            {
                                Key = legalProvision.Title,
                                Shortname = $"A{AppendixCounter}",
                                Description = legalProvision.Title,
                                FileDescription = legalProvision.Title,
                                Url = legalProvision.Url
                            };

                            if (TocAppendixes.Any(x => x.Url == tocAppendix.Url && x.Key == tocAppendix.Key))
                            {
                                tocAppendix = TocAppendixes.First(x => x.Url == tocAppendix.Url && x.Key == tocAppendix.Key);
                                tocItem.Appendixes.Add(tocAppendix);
                                continue;
                            }

                            var urlFile = tocAppendix.Url;
                            var directory = Path.Combine(Path.GetTempPath(), $"_TempFile_{Guid.NewGuid()}");

                            Directory.CreateDirectory(directory);

                            var filepath = Path.Combine(directory, "output.bin");
                            tocAppendix.Filename = filepath;

                            var result = Oereb.Report.Helper.Content.GetFromUrl(urlFile, filepath);

                            tocAppendix.ContentType = result.ContentType;
                            tocAppendix.State = result.Successful;

                            if (AttacheFiles && result.Successful)
                            {
                                tocAppendix.Description += "(siehe im Anhang)";
                            }
                            else if (AttacheFiles && !result.Successful)
                            {
                                tocAppendix.Description += "(nicht anhängbar)";
                            }
                            else if (result.Successful)
                            {
                                var files = Oereb.Report.Helper.Pdf.GetImagesFromPpdf(filepath);
                                tocAppendix.Pages.AddRange(files);
                            }

                            tocItem.Appendixes.Add(tocAppendix);
                            TocAppendixes.Add(tocAppendix);
                            AppendixCounter++;
                        }
                    }
                }

                Toc.TocItems.Add(tocItem);
            }

            foreach (var notConcernedTheme in Extract.NotConcernedTheme)
            {
                var localisedText = new Service.DataContracts.Model.v04.LocalisedText[] { notConcernedTheme.Text };
                var label = Helper.LocalisedText.GetStringFromArray(localisedText, Language);               
                Toc.ThemeNotConcerned.Add(new TocItemTheme() {Label = label });
            }

            foreach (var themeWithoutData in Extract.ThemeWithoutData)
            {
                var localisedText = new Service.DataContracts.Model.v04.LocalisedText[] { themeWithoutData.Text };
                var label = Helper.LocalisedText.GetStringFromArray(localisedText, Language);
                Toc.ThemeWithoutData.Add(new TocItemTheme() { Label = label });
            }
        }

        public class TocRegion
        {
            public Service.DataContracts.Model.v04.Extract Extract { get; set; }
            public string Language { get; set; }

            public List<TocItem> TocItems { get; set; }
            public List<TocItemTheme> ThemeNotConcerned { get; set; }
            public List<TocItemTheme> ThemeWithoutData { get; set; }

            public string GeneralInformation { get; set; }
            public string BaseData { get; set; }
            public string ExclusionOfLiabilityTitle { get; set; }
            public string ExclusionOfLiabilityContent { get; set; }

            public TocRegion()
            {
                ThemeWithoutData = new List<TocItemTheme>();
                ThemeNotConcerned = new List<TocItemTheme>();
                TocItems = new List<TocItem>();
            }
        }

        public class TocItem
        {
            public int Page { get; set; }
            public string Label { get; set; }
            public List<TocAppendix> Appendixes { get; set; }

            public TocItem()
            {
                Appendixes = new List<TocAppendix>();
            }
        }

        public class TocAppendix
        {
            public string Shortname { get; set; }
            public string Description { get; set; }
            public string Url { get; set; }
            public List<byte[]> Pages { get; set; }
            public string Filename { get; set; }
            public string Key { get; set; }
            public bool State { get; set; }
            public string ContentType { get; set; }
            public string FileDescription { get; set; }

            public TocAppendix()
            {
                Pages = new List<byte[]>();
            }
        }

        public class TocItemTheme
        {
            public string Label { get; set; }
        }

        #endregion

        #region report body

        public void IniReportBody()
        {
            ReportBodyItems = new List<BodyItem>();

            var groupedBodyItems = Extract.RealEstate.RestrictionOnLandownership.GroupBy(x => x.Theme.Code + "|" + x.SubTheme ?? "").ToList();

            int section = -1;

            foreach (var bodyItem in groupedBodyItems)
            {
                section++;

                if (!(BodySectionFlag == section || BodySectionFlag == -1))
                {
                    continue;
                }

                ReportBodyItems.Add(new BodyItem(Extract, bodyItem.ToList(), Language, ImageRestrictionOnLandownership, AdditionalLayers));
            }
        }

        public class BodyItem
        {
            public byte[] FederalLogo { get; set; }
            public byte[] CantonalLogo { get; set; }
            public byte[] MunicipalityLogo { get; set; }
            public byte[] LogoPLRCadastre { get; set; }
            public string Theme { get; set; }
            public Image Image { get; set; }
            public List<LegendItem> LegendItems { get; set; }
            public List<LegendItem> LegendItemsNotInvolved
            {
                get
                {
                    var legendItemsNotInvolved = new List<LegendItem>();

                    foreach (var legendItem in LegendItems)
                    {
                        if (LegendItemsInvolved.Any(x => legendItem.TypeCode == x.TypeCode))
                        {
                            continue; //involved
                        }

                        if (legendItemsNotInvolved.Any(x => legendItem.TypeCode == x.TypeCode))
                        {
                            continue; //already exist
                        }
                        legendItemsNotInvolved.Add(legendItem);
                    }

                    return legendItemsNotInvolved;
                }
            }
            public List<LegendItemInvolved> LegendItemsInvolved { get; set; }
            public List<LegendAtWeb> LegendAtWeb { get; set; }
            public bool VisibleLegendAtWeb => LegendAtWeb.Any();
            public List<Document> LegalProvisions { get; set; }
            public List<Document> Documents { get; set; }
            public List<ResponsibleOffice> ResponsibleOffice { get; set; }
            public string ExtractIdentifier { get; set; }
            public DateTime CreationDate { get; set; }

            public BodyItem(Service.DataContracts.Model.v04.Extract extract, List<Service.DataContracts.Model.v04.RestrictionOnLandownership> restrictionOnLandownership, string language, ImageExtension baselayer, List<ImageExtension> additionalLayers)
            {
                #region initialization

                LegendItems = new List<LegendItem>();
                LegendItemsInvolved = new List<LegendItemInvolved>();
                LegendAtWeb = new List<LegendAtWeb>();

                LegalProvisions = new List<Document>();
                Documents = new List<Document>();

                ResponsibleOffice = new List<ResponsibleOffice>();

                #endregion

                FederalLogo = (byte[]) extract.Item1;
                CantonalLogo = (byte[])extract.Item2;
                MunicipalityLogo = (byte[])extract.Item3;
                LogoPLRCadastre = (byte[])extract.Item;
                ExtractIdentifier = extract.ExtractIdentifier;
                CreationDate = extract.CreationDate;

                Theme = restrictionOnLandownership.First().Theme.Text.Text;

                var image = PreProcessing.GetImageFromByteArray(restrictionOnLandownership.First().Map.Image); //take only with and height from image
                Image = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);

                var rolLayers = new List<ImageExtension>();

                rolLayers.Add(baselayer);
                rolLayers.AddRange(additionalLayers.Where(x=> x.Name == Theme));

                foreach (var restriction in restrictionOnLandownership)
                {
                    var restrictionExtension = restriction.Map.extensions?.Any.FirstOrDefault(x => x.LocalName == "MapExtension");
                    int seq = 5;
                    double transparency = 0.5;

                    if (restrictionExtension != null)
                    {
                        var restrictionExtensionElement = XElement.Parse(restrictionExtension.OuterXml);

                        seq = restrictionExtensionElement.Element("Seq") == null ? 5 : System.Convert.ToInt32(restrictionExtensionElement.Element("Seq").Value);
                        transparency = restrictionExtensionElement.Element("Transparency") == null ? 0.5 : System.Convert.ToDouble(restrictionExtensionElement.Element("Transparency").Value);
                    }

                    var imageExtension = new ImageExtension() {Image = PreProcessing.GetImageFromByteArray(restriction.Map.Image), Seq = seq, Transparency = transparency};
                    rolLayers.Add(imageExtension);
                }

                var sortedRolLayers = rolLayers.OrderBy(x => x.Seq);
                var overview = sortedRolLayers.Select(x => $"{x.Seq}, {x.Name}");

                foreach (var rolLayer in sortedRolLayers)
                {
                    Image = PreProcessing.MergeTwoImages(Image, PreProcessing.SetImageOpacity(rolLayer.Image, (float)(1-rolLayer.Transparency)));
                }

                if (baselayer != null && baselayer.Extent != null)
                {
                    var parcelHighligthed = Helper.Geometry.RasterizeGeometryFromGml(
                        extract.RealEstate.Limit,
                        new double[] { baselayer.Extent.Xmin, baselayer.Extent.Ymin, baselayer.Extent.Xmax, baselayer.Extent.Ymax },
                        image.Width,
                        image.Height
                    );

                    Image = PreProcessing.MergeTwoImages(Image, parcelHighligthed);
                }

                foreach (var restriction in restrictionOnLandownership)
                {

                    #region  legenditems

                    if (restriction.Map.OtherLegend != null)
                    {
                        foreach (var legend in restriction.Map.OtherLegend)
                        {
                            LegendItems.Add(new LegendItem()
                            {
                                Symbol = (byte[])legend.Item,
                                TypeCode = legend.TypeCode,
                                Label = legend.LegendText.FirstOrDefault(x => x.Language.ToString() == language) != null ? legend.LegendText.First(x => x.Language.ToString() == language).Text : "-"
                            });
                        }
                    }

                    var legendItemCatched = LegendItems.FirstOrDefault(x => x.TypeCode == restriction.TypeCode);

                    var geometryExtension = restriction.Geometry.First().extensions.Any.FirstOrDefault(x => x.LocalName == "GeometryExtension");
                    var type = "NoExtension";

                    if (geometryExtension != null)
                    {
                        var geometryExtentionElement = XElement.Parse(geometryExtension.OuterXml);
                        var geometryExtention = GetGeometryExtension(geometryExtentionElement);
                        type = geometryExtention.Type;
                    }

                    if (legendItemCatched != null)
                    {
                        var area = "";
                        var partInPercent = "";

                        if (type == "Polygon" || type == "NoExtension")
                        {
                            area = restriction.Area + " m²";

                            if (restriction.PartInPercent == "0")
                            {
                                partInPercent = "< 1 %";
                            }
                            else
                            {
                                partInPercent = restriction.PartInPercent + " %";
                            }
                        }
                        
                        var legendInvolved = new LegendItemInvolved()
                        {
                            Symbol = legendItemCatched.Symbol,
                            TypeCode = legendItemCatched.TypeCode,
                            Label = legendItemCatched.Label,
                            Area = area,
                            PartInPercent = partInPercent
                        };

                        LegendItemsInvolved.Add(legendInvolved);
                    }

                    #endregion

                    #region legend at web

                    if (restriction.Map.LegendAtWeb != null && !string.IsNullOrEmpty(restriction.Map.LegendAtWeb.Value) && !LegendAtWeb.Any(x => x.Url == restriction.Map.LegendAtWeb.Value))
                    {
                        LegendAtWeb.Add(new LegendAtWeb() { Label = restriction.Map.LegendAtWeb.Value, Url = restriction.Map.LegendAtWeb.Value });
                    }

                    #endregion

                    #region legalprovisions

                    if (restriction.LegalProvisions != null)
                    {
                        foreach (var document in restriction.LegalProvisions.OfType<Service.DataContracts.Model.v04.LegalProvisions>())
                        {
                            var documentItem = new Document()
                            {
                                Title = Helper.LocalisedText.GetStringFromArray(document.Title, language),
                                Abbrevation = Helper.LocalisedText.GetStringFromArray(document.Abbrevation, language),
                                OfficialNumber = string.IsNullOrEmpty(document.OfficialNumber) ? "" : document.OfficialNumber + " ",
                                OfficialTitle = Helper.LocalisedText.GetStringFromArray(document.OfficialTitle, language),
                                Url = WebUtility.HtmlEncode(Helper.LocalisedUri.GetStringFromArray(document.TextAtWeb, language))
                            };

                            if (LegalProvisions.Any(x => x.Id == documentItem.Id))
                            {
                                continue;
                            }

                            LegalProvisions.Add(documentItem);
                        }
                    }

                    #endregion

                    #region documents

                    if (restriction.LegalProvisions != null)
                    {
                        foreach (var document in restriction.LegalProvisions.OfType<Service.DataContracts.Model.v04.Document>().Where(x => !(x is Service.DataContracts.Model.v04.LegalProvisions)))
                        {
                            var documentItem = new Document()
                            {
                                Title = Helper.LocalisedText.GetStringFromArray(document.Title, language),
                                Abbrevation = Helper.LocalisedText.GetStringFromArray(document.Abbrevation, language),
                                OfficialNumber = string.IsNullOrEmpty(document.OfficialNumber) ? "" : document.OfficialNumber + " ",
                                OfficialTitle = Helper.LocalisedText.GetStringFromArray(document.OfficialTitle, language),
                                Url = WebUtility.HtmlEncode(Helper.LocalisedUri.GetStringFromArray(document.TextAtWeb, language)),
                                Level = !String.IsNullOrEmpty(document.Municipality) ? 2 : document.CantonSpecified ? 1 : 0
                            };

                            if (Documents.Any(x => x.Id == documentItem.Id))
                            {
                                continue;
                            }

                            Documents.Add(documentItem);
                        }
                    }

                    #endregion

                    #region responsible office

                    var responsibleOffice = new ResponsibleOffice()
                    {
                        Name = Helper.LocalisedText.GetStringFromArray(restriction.ResponsibleOffice.Name, language),
                        Url = restriction.ResponsibleOffice.OfficeAtWeb.Value
                    };

                    if (!ResponsibleOffice.Any(x=> x.Id == responsibleOffice.Id))
                    {
                        ResponsibleOffice.Add(responsibleOffice);
                    }

                    #endregion
                }
            }
        }

        #region helper classes report body

        public class LegendItem
        {
            public string TypeCode { get; set; }
            public byte[] Symbol { get; set; }
            public string Label { get; set; }
        }

        public class LegendItemInvolved : LegendItem
        {
            public string Area { get; set; }
            public string PartInPercent { get; set; }
        }

        public class LegendAtWeb
        {
            public string Url { get; set; }
            public string Label { get; set; }
        }

        public class Document
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public string OfficialTitle { get; set; }
            public string Abbrevation { get; set; }
            public string OfficialNumber { get; set; }
            public int Level { get; set; } //for sorting 0 federal | 1 canton | 2 municipality

            public string Id => $"{Title}|{OfficialTitle}|{OfficialNumber}|{Url}";
        }

        public class ResponsibleOffice
        {
            public string Name { get; set; }
            public string Url { get; set; }

            public string Id => $"{Name}|{Url}";
        }

        #endregion //helper classes report body

        #endregion //report body

        #region report glossary

        public void IniReportGlossary()
        {
            GlossaryItems = new List<GlossaryItem>();

            GlossaryItems.AddRange(Extract.Glossary.Where(x=>x.Title.Any(y=>y.Language.ToString() == Language)).Select(x => new GlossaryItem()
            {
                Abbreviation = Helper.LocalisedText.GetStringFromArray(x.Title, Language),
                Description = Helper.LocalisedMText.GetStringFromArray(x.Content, Language),
            }));
        }

        public class GlossaryItem
        {
            public string Abbreviation { get; set; }
            public string Description { get; set; }
        }

        #endregion
    }
}
