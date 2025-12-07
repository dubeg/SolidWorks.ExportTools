using System;
using System.Collections.Generic;
using System.Linq;
using ACadSharp.IO;
using ACadSharp;

namespace Dubeg.Sw.ExportTools.Commands.DwgToPdf;

public class DwgImportInfo {
    private const string BLOCK_MODEL = "MODEL";
    private const string BLOCK_PAPER_SPACE = "PAPER_SPACE";
    private const string BLOCK_CART = "CART";
    private const string BLOCK_VUE_ISO = "VUEISOME";

    public string Filename { get; set; }
    public string SheetName { get; set; }
    public string[] SheetNames { get; set; }
    public List<SheetInfo> SheetInfos { get; set; }

    public DwgImportInfo(string filename) {
        Filename = filename;
        SheetName = string.Empty;
        SheetNames = [];
    }

    public class SheetInfo {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool HasIsoView { get; set; }
        public bool HasCart { get; set; }
        public string CartName { get; set; }
    }

    public void Load() {
        if (string.IsNullOrEmpty(Filename)) {
            throw new ArgumentNullException("Filename is not set.");
        }
        var doc = DwgReader.Read(Filename);
        var sheetInfos = new List<SheetInfo>();
        foreach (var layout in doc.Layouts) {
            var inserts = layout.AssociatedBlock.Entities
                .Where(x => x.ObjectType == ObjectType.INSERT)
                .Cast<ACadSharp.Entities.Insert>();
            
            var vueIso = inserts
                .Where(y => y.Block.Name == BLOCK_VUE_ISO)
                .Select(y => y.Block)
                .FirstOrDefault();
            
            var cart = inserts
               .Where(y => y.Block.Name.ToUpper().Contains(BLOCK_CART))
               .Select(y => y.Block)
               .FirstOrDefault();

            var layoutBlockName = layout.AssociatedBlock.Name.ToUpper();
            var layoutType = 
                layoutBlockName.Contains(BLOCK_PAPER_SPACE) ? "Paper":
                layoutBlockName.Contains(BLOCK_MODEL) ? "Model" : 
                "?";

            sheetInfos.Add(
                new SheetInfo() { 
                    Name = layout.Name, 
                    Type = layoutType,
                    HasIsoView = vueIso is not null, 
                    HasCart = cart is not null, 
                    CartName = cart?.Name ?? string.Empty 
                }
            );
        }
        SheetInfos = sheetInfos;
    }
}
