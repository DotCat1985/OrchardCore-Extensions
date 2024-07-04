using DotCat.OrchardCore.Liquid.TryItEditor;
using OrchardCore.Modules.Manifest;
using DotCatManifest = DotCat.OrchardCore.Manifest;

[assembly: Module(
    Name = TryItEditorConstants.Features.TryItEditor,
    Author = DotCatManifest.ManifestConstants.Author,
    Version = DotCatManifest.ManifestConstants.Version,
    Description = "Liquid TryIt editor for OrchardCore",
    Category = "Content Management",
    Dependencies = new[] { "OrchardCore.Liquid" }
)]
