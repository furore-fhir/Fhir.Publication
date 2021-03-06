﻿/*
Copyright (c) 2011+, HL7, Inc
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.
 * Neither the name of HL7 nor the names of its contributors may be used to 
   endorse or promote products derived from this software without specific 
   prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.

*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Hl7.Fhir.Support;

namespace Hl7.Fhir.Publication
{
     internal class HierarchicalTableGenerator
    {
        private ProfileKnowledgeProvider _pkp;

        public HierarchicalTableGenerator(ProfileKnowledgeProvider pkp)
        {
            _pkp = pkp;
        }

        public XElement generate(TableModel model)
        {
            checkModel(model);
            var table = new XElement(XmlNs.XHTMLNS + "table",
                    new XAttribute("border", "0"),
                    new XAttribute("cellspacing", "0"),
                    new XAttribute("cellpadding", "0"),
                    new XAttribute("style", "border: 0px; font-size: 11px; font-family: verdana; vertical-align: top;"));

            var tr = new XElement(XmlNs.XHTMLNS + "tr",
                new XAttribute("style", "border: 1px #F0F0F0 solid; font-size: 11px; font-family: verdana; vertical-align: top;"));

            table.Add(tr);

            foreach (Title t in model.Titles)
            {
                var tc = renderCell(tr, t, "th", null, null, false, null);
            }

            foreach (Row r in model.Rows)
            {
                renderRow(table, r, 0, new List<bool>());
            }

            return table;
        }


        private void renderRow(XElement table, Row r, int indent, List<Boolean> indents)
        {
            var tr = new XElement(XmlNs.XHTMLNS + "tr",
                new XAttribute("style", "border: 0px; padding:0px; vertical-align: top; background-color: white;"));
            table.Add(tr);

            bool first = true;
            foreach (Cell t in r.getCells())
            {
                renderCell(tr, t, "td", first ? r.getIcon() : null, first ? indents : null, r.getSubRows().Any(), first ? r.getAnchor() : null);
                first = false;
            }

            // table.addText("\r\n");

            for (int i = 0; i < r.getSubRows().Count; i++)
            {
                Row c = r.getSubRows()[i];
                var ind = new List<Boolean>();
                ind.AddRange(indents);
                if (i == r.getSubRows().Count - 1)
                    ind.Add(true);
                else
                    ind.Add(false);
                renderRow(table, c, indent + 1, ind);
            }
        }


        private XElement renderCell(XElement tr, Cell c, String name, String icon, List<Boolean> indents, bool hasChildren, String anchor)
        {
            var tc = new XElement(XmlNs.XHTMLNS + name, new XAttribute("class", "heirarchy"));
            tr.Add(tc);

            if (indents != null)
            {
                var spacerImg = new XElement(XmlNs.XHTMLNS + "img",
                   new XAttribute("src", srcFor("tbl_spacer.png")),
                   new XAttribute("class", "heirarchy"),
                   new XAttribute("alt", "."));
                tc.Add(spacerImg);

                tc.Add(new XAttribute("style", "vertical-align: top; text-align : left; padding:0px 4px 0px 4px; white-space: nowrap; background-image: url("
                          + checkExists(indents, hasChildren) + ")"));

                for (int i = 0; i < indents.Count - 1; i++)
                {
                    if (indents[i])
                    {
                        var blankImg = new XElement(XmlNs.XHTMLNS + "img",
                            new XAttribute("src", srcFor("tbl_blank.png")),
                            new XAttribute("class", "heirarchy"),
                            new XAttribute("alt", "."));
                        tc.Add(blankImg);
                    }
                    else
                    {
                        var vlineImage = new XElement(XmlNs.XHTMLNS + "img",
                            new XAttribute("src", srcFor("tbl_vline.png")),
                            new XAttribute("class", "heirarchy"),
                            new XAttribute("alt", "."));
                        tc.Add(vlineImage);
                    }
                }

                if (indents.Any())
                {
                    if (indents[indents.Count - 1])
                    {
                        var vjoinEndImage = new XElement(XmlNs.XHTMLNS + "img",
                            new XAttribute("src", srcFor("tbl_vjoin_end.png")),
                            new XAttribute("class", "heirarchy"),
                            new XAttribute("alt", "."));
                        tc.Add(vjoinEndImage);
                    }
                    else
                    {
                        var vjoinImage = new XElement(XmlNs.XHTMLNS + "img",
                            new XAttribute("src", srcFor("tbl_vjoin.png")),
                            new XAttribute("class", "heirarchy"),
                            new XAttribute("alt", "."));
                        tc.Add(vjoinImage);
                    }
                }
            }
            else
            {
                string style = "vertical-align:top; text-align:left; padding:0px 4px 0px 4px";

                if (c is Title)
                {
                    var width = ((Title)c).width;

                    if (width != 0) style += "; width:" + width.ToString() + "px";
                }

                tc.Add(new XAttribute("style", style));
            }

            if (!String.IsNullOrEmpty(icon))
            {
                tc.Add(new XElement(XmlNs.XHTMLNS + "img",
                    new XAttribute("src", srcFor(icon)),
                    new XAttribute("class", "heirarchy"),
                    new XAttribute("style", "background-color: white;"),
                    new XAttribute("alt", ".")));
                //tc.addText(" ");
            }

            foreach (Piece p in c.pieces)
            {
                if (!String.IsNullOrEmpty(p.getTag()))
                {
                    var newTag = new XElement(XmlNs.XHTMLNS + p.getTag());
                    tc.Add(newTag);
                    addStyle(newTag, p);
                }
                else if (!String.IsNullOrEmpty(p.getReference()))
                {
                    var newTag = new XElement(XmlNs.XHTMLNS + "a");
                    tc.Add(newTag);
                    XElement a = addStyle(newTag, p);
                    a.Add(new XAttribute("href", p.getReference()));

                    if (!String.IsNullOrEmpty(p.getHint()))
                        a.Add(new XAttribute("title", p.getHint()));

                    a.Add(new XText(p.getText()));
                }
                else
                {
                    if (!String.IsNullOrEmpty(p.getHint()))
                    {
                        var span = new XElement(XmlNs.XHTMLNS + "span");
                        tc.Add(span);

                        XElement s = addStyle(span, p);
                        s.Add(new XAttribute("title", p.getHint()));
                        s.Add(new XText(p.getText()));
                    }
                    else if (p.getStyle() != null)
                    {
                        var span = new XElement(XmlNs.XHTMLNS + "span");
                        tc.Add(span);
                        XElement s = addStyle(span, p);
                        var text = p.getText() ?? "(empty?)";
                        s.Add(new XText(text));
                    }
                    else
                        tc.Add(new XText(p.getText() ?? "(empty?)"));
                }
            }
            if (!String.IsNullOrEmpty(anchor))
            {
                var a = new XElement(XmlNs.XHTMLNS + "a",
                            new XAttribute("name", nmTokenize(anchor)));
                // .addText(" ")
                tc.Add(a);
            }

            return tc;
        }


        private XElement addStyle(XElement node, Piece p)
        {
            if (p.getStyle() != null)
                node.Add(new XAttribute("style", p.getStyle()));
            return node;
        }

        private String nmTokenize(String anchor)
        {
            return anchor.Replace("[", "_").Replace("]", "_");
        }


        private String srcFor(String filename)
        {
            var imgPath = "../dist/images/" + filename;

            if (_pkp.InlineGraphics)
            {
                StringBuilder b = new StringBuilder();
                b.Append("data: image/png;base64,");
                byte[] bytes = File.ReadAllBytes(imgPath);

                b.Append(Convert.ToBase64String(bytes));
                return b.ToString();
            }
            else
                return imgPath;
        }


        private void checkModel(TableModel model)
        {
            check(model.Rows.Any(), "Must have rows");
            check(model.Titles.Any(), "Must have titles");
            foreach (Cell c in model.Titles)
                check(c);
            foreach (Row r in model.Rows)
                check(r, "rows", model.Titles.Count);
        }


        private void check(Cell c)
        {
            bool hasText = false;
            foreach (Piece p in c.pieces)
                if (!String.IsNullOrEmpty(p.getText()))
                    hasText = true;
            check(hasText, "Title cells must have text");
        }


        private void check(Row r, String str, int size)
        {
            check(r.getCells().Count == size, "All rows must have the same number of columns as the titles");
            foreach (Row c in r.getSubRows())
                check(c, "rows", size);
        }


        private String checkExists(List<Boolean> indents, bool hasChildren)
        {
            StringBuilder b = new StringBuilder();

            if (_pkp.InlineGraphics)
            {
                MemoryStream bytes = new MemoryStream();
                genImage(indents, hasChildren, bytes);
                b.Append("data: image/png;base64,");
                var encodeBase64 = Convert.ToBase64String(bytes.ToArray());
                b.Append(encodeBase64);
            }
            else
            {
                b.Append("tbl_bck");
                foreach (Boolean i in indents)
                    b.Append(i ? "0" : "1");

                if (hasChildren)
                    b.Append("1");
                else
                    b.Append("0");

                b.Append(".png");

                String file = Path.Combine(_pkp.ImageOutputDirectory, b.ToString());

                if (!File.Exists(file))
                {
                    var stream = new FileStream(file, FileMode.Create);
                    genImage(indents, hasChildren, stream);
                }
            }

            return Path.Combine(_pkp.ImageLinkPath, b.ToString());
        }


        private void genImage(List<Boolean> indents, bool hasChildren, Stream stream)
        {
            var bi = new Bitmap(400, 2);
            var graphics = Graphics.FromImage(bi);

            graphics.DrawRectangle(new Pen(Color.White), 0, 0, 400, 2);

            for (int i = 0; i < indents.Count; i++)
            {
                if (!indents[i])
                    bi.SetPixel(12 + (i * 16), 0, Color.Black);
            }

            if (hasChildren)
            {
                bi.SetPixel(12 + (indents.Count * 16), 0, Color.Black);
            }

            bi.Save(stream, ImageFormat.Png);
        }


        private void check(bool check, String message)
        {
            if (!check)
                throw new Exception(message);
        }
    }


     internal class Piece
     {
         private String tag;
         private String reference;
         private String text;
         private String hint;
         private String style;

         public Piece(String tag)
         {
             this.tag = tag;
         }

         public Piece(String reference, String text, String hint)
         {
             this.reference = reference;
             this.text = text;
             this.hint = hint;
         }
         public String getReference()
         {
             return reference;
         }
         public void setReference(String value)
         {
             reference = value;
         }
         public String getText()
         {
             return text;
         }
         public String getHint()
         {
             return hint;
         }

         public String getTag()
         {
             return tag;
         }

         public String getStyle()
         {
             return style;
         }

         public Piece setStyle(String style)
         {
             this.style = style;
             return this;
         }

         public Piece addStyle(String style)
         {
             if (this.style != null)
                 this.style = this.style + ": " + style;
             else
                 this.style = style;
             return this;
         }

     }


     internal class Cell
     {
         internal List<Piece> pieces = new List<Piece>();

         public Cell()
         {
         }

         public Cell(String prefix, String reference, String text, String hint, String suffix)
         {

             if (!String.IsNullOrEmpty(prefix)) pieces.Add(new Piece(null, prefix, null));
             pieces.Add(new Piece(reference, text, hint));
             if (!String.IsNullOrEmpty(suffix)) pieces.Add(new Piece(null, suffix, null));
         }

         public List<Piece> getPieces()
         {
             return pieces;
         }
         public Cell addPiece(Piece piece)
         {
             pieces.Add(piece);
             return this;
         }
     }



     internal class Title : Cell
     {
         internal int width;

         public Title(String prefix, String reference, String text, String hint, String suffix, int width)
             : base(prefix, reference, text, hint, suffix)
         {

             this.width = width;
         }
     }


     internal class Row
     {
         private List<Row> subRows = new List<Row>();
         private List<Cell> cells = new List<Cell>();
         private String icon;
         private String anchor;

         public List<Row> getSubRows()
         {
             return subRows;
         }
         public List<Cell> getCells()
         {
             return cells;
         }
         public String getIcon()
         {
             return icon;
         }
         public void setIcon(String icon)
         {
             this.icon = icon;
         }
         public String getAnchor()
         {
             return anchor;
         }
         public void setAnchor(String anchor)
         {
             this.anchor = anchor;
         }


     }


     internal class TableModel
     {
         public static TableModel CreateNormalTable()
         {
             TableModel model = new TableModel();

             model.Titles.Add(new Title(null, null, "Name", null, null, 0));
             model.Titles.Add(new Title(null, null, "Card.", null, null, 0));
             model.Titles.Add(new Title(null, null, "Type", null, null, 100));
             model.Titles.Add(new Title(null, null, "Description & Constraints", null, null, 0));
             return model;
         }



         public List<Title> Titles = new List<Title>();
         public List<Row> Rows = new List<Row>();
     }
}


#if ORIGINAL_JAVA_CODE
package org.hl7.fhir.utilities.xhtml;

/*
Copyright (c) 2011+, HL7, Inc
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

 * Redistributions of source code must retain the above copyright notice, this 
   list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, 
   this list of conditions and the following disclaimer in the documentation 
   and/or other materials provided with the distribution.
 * Neither the name of HL7 nor the names of its contributors may be used to 
   endorse or promote products derived from this software without specific 
   prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
POSSIBILITY OF SUCH DAMAGE.

*/

import java.awt.Color;
import java.awt.Graphics2D;
import java.awt.image.BufferedImage;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.util.ArrayList;
import java.util.List;
import javax.imageio.ImageIO;
import org.hl7.fhir.utilities.Utilities;
import org.apache.commons.codec.binary.Base64;
import org.apache.commons.io.FileUtils;


public class HeirarchicalTableGenerator  {

  public class Piece {
    private String tag;
    private String reference;
    private String text;
    private String hint;
    private String style;
    
    public Piece(String tag) {
      super();
      this.tag = tag;
    }
    
    public Piece(String reference, String text, String hint) {
      super();
      this.reference = reference;
      this.text = text;
      this.hint = hint;
    }
    public String getReference() {
      return reference;
    }
    public void setReference(String value) {
      reference = value;
    }
    public String getText() {
      return text;
    }
    public String getHint() {
      return hint;
    }

    public String getTag() {
      return tag;
    }

    public String getStyle() {
      return style;
    }

    public Piece setStyle(String style) {
      this.style = style;
      return this;
    }

    public Piece addStyle(String style) {
      if (this.style != null)
        this.style = this.style+": "+style;
      else
        this.style = style;
      return this;
    }

  }
  
  public class Cell {
    private List<Piece> pieces = new ArrayList<HeirarchicalTableGenerator.Piece>();

    public Cell() {
      
    }
    public Cell(String prefix, String reference, String text, String hint, String suffix) {
      super();
      if (!Utilities.noString(prefix))
        pieces.add(new Piece(null, prefix, null));
      pieces.add(new Piece(reference, text, hint));
      if (!Utilities.noString(suffix))
        pieces.add(new Piece(null, suffix, null));
    }
    public List<Piece> getPieces() {
      return pieces;
    }
    public Cell addPiece(Piece piece) {
      pieces.add(piece);
      return this;
    }
  }

  public class Title extends Cell {
    private int width;

    public Title(String prefix, String reference, String text, String hint, String suffix, int width) {
      super(prefix, reference, text, hint, suffix);
      this.width = width;
    }
  }
  
  public class Row {
    private List<Row> subRows = new ArrayList<HeirarchicalTableGenerator.Row>();
    private List<Cell> cells = new ArrayList<HeirarchicalTableGenerator.Cell>();
    private String icon;
    private String anchor;
    
    public List<Row> getSubRows() {
      return subRows;
    }
    public List<Cell> getCells() {
      return cells;
    }
    public String getIcon() {
      return icon;
    }
    public void setIcon(String icon) {
      this.icon = icon;
    }
    public String getAnchor() {
      return anchor;
    }
    public void setAnchor(String anchor) {
      this.anchor = anchor;
    }
    
    
  }

  public class TableModel {
    private List<Title> titles = new ArrayList<HeirarchicalTableGenerator.Title>();
    private List<Row> rows = new ArrayList<HeirarchicalTableGenerator.Row>();
    public List<Title> getTitles() {
      return titles;
    }
    public List<Row> getRows() {
      return rows;
    }
  }


  private String dest;
  
  /**
   * There are circumstances where the table has to present in the absence of a stable supporting infrastructure.
   * and the file paths cannot be guaranteed. For these reasons, you can tell the builder to inline all the graphics
   * (all the styles are inlined anyway, since the table fbuiler has even less control over the styling
   *  
   */
  private boolean inLineGraphics;
  
  
  public HeirarchicalTableGenerator(String dest, boolean inlineGraphics) {
    super();
    this.dest = dest;
    this.inLineGraphics = inlineGraphics;
  }

  public TableModel initNormalTable() {
    TableModel model = new TableModel();
    
    model.getTitles().add(new Title(null, null, "Name", null, null, 0));
    model.getTitles().add(new Title(null, null, "Card.", null, null, 0));
    model.getTitles().add(new Title(null, null, "Type", null, null, 100));
    model.getTitles().add(new Title(null, null, "Description & Constraints", null, null, 0));
    return model;
  }

  public XhtmlNode generate(TableModel model) throws Exception {
    checkModel(model);
    XhtmlNode table = new XhtmlNode(NodeType.Element, "table").setAttribute("border", "0").setAttribute("cellspacing", "0").setAttribute("cellpadding", "0");
    table.setAttribute("style", "border: 0px; font-size: 11px; font-family: verdana; vertical-align: top;");
    XhtmlNode tr = table.addTag("tr");
    tr.setAttribute("style", "border: 1px #F0F0F0 solid; font-size: 11px; font-family: verdana; vertical-align: top;");
    for (Title t : model.getTitles()) {
      XhtmlNode tc = renderCell(tr, t, "th", null, null, false, null);
      if (t.width != 0)
        tc.setAttribute("style", "width: "+Integer.toString(t.width)+"px");
    }
    for (Row r : model.getRows()) {
      renderRow(table, r, 0, new ArrayList<Boolean>());
    }
    return table;
  }


  private void renderRow(XhtmlNode table, Row r, int indent, List<Boolean> indents) throws Exception {
    XhtmlNode tr = table.addTag("tr");
    tr.setAttribute("style", "border: 0px; padding:0px; vertical-align: top; background-color: white;");
    boolean first = true;
    for (Cell t : r.getCells()) {
      renderCell(tr, t, "td", first ? r.getIcon() : null, first ? indents : null, !r.getSubRows().isEmpty(), first ? r.getAnchor() : null);
      first = false;
    }
    table.addText("\r\n");
    
    for (int i = 0; i < r.getSubRows().size(); i++) {
      Row c = r.getSubRows().get(i);
      List<Boolean> ind = new ArrayList<Boolean>();
      ind.addAll(indents);
      if (i == r.getSubRows().size() - 1)
        ind.add(true);
      else
        ind.add(false);
      renderRow(table, c, indent+1, ind);
    }
  }


  private XhtmlNode renderCell(XhtmlNode tr, Cell c, String name, String icon, List<Boolean> indents, boolean hasChildren, String anchor) throws Exception {
    XhtmlNode tc = tr.addTag(name);
    tc.setAttribute("class", "heirarchy");
    if (indents != null) {
      tc.addTag("img").setAttribute("src", srcFor("tbl_spacer.png")).setAttribute("class", "heirarchy").setAttribute("alt", ".");
      tc.setAttribute("style", "vertical-align: top; text-align : left; padding:0px 4px 0px 4px; white-space: nowrap; background-image: url("+checkExists(indents, hasChildren)+")");
      for (int i = 0; i < indents.size()-1; i++) { 
        if (indents.get(i))
          tc.addTag("img").setAttribute("src", srcFor("tbl_blank.png")).setAttribute("class", "heirarchy").setAttribute("alt", ".");
        else
          tc.addTag("img").setAttribute("src", srcFor("tbl_vline.png")).setAttribute("class", "heirarchy").setAttribute("alt", ".");
      }
      if (!indents.isEmpty())
        if (indents.get(indents.size()-1))
          tc.addTag("img").setAttribute("src", srcFor("tbl_vjoin_end.png")).setAttribute("class", "heirarchy").setAttribute("alt", ".");
        else
          tc.addTag("img").setAttribute("src", srcFor("tbl_vjoin.png")).setAttribute("class", "heirarchy").setAttribute("alt", ".");
    }
    else
      tc.setAttribute("style", "vertical-align: top; text-align : left; padding:0px 4px 0px 4px");
    if (!Utilities.noString(icon)) {
      tc.addTag("img").setAttribute("src", srcFor(icon)).setAttribute("class", "heirarchy").setAttribute("style", "background-color: white;").setAttribute("alt", ".");
      tc.addText(" ");
    }
    for (Piece p : c.pieces) {
      if (!Utilities.noString(p.getTag())) {
        addStyle(tc.addTag(p.getTag()), p);
      } else if (!Utilities.noString(p.getReference())) {
        XhtmlNode a = addStyle(tc.addTag("a"), p);
        a.setAttribute("href", p.getReference());
        if (!Utilities.noString(p.getHint()))
          a.setAttribute("title", p.getHint());
        a.addText(p.getText());
      } else { 
        if (!Utilities.noString(p.getHint())) {
          XhtmlNode s = addStyle(tc.addTag("span"), p);
          s.setAttribute("title", p.getHint());
          s.addText(p.getText());
        } else if (p.getStyle() != null) {
          XhtmlNode s = addStyle(tc.addTag("span"), p);
          s.addText(p.getText());
        } else
          tc.addText(p.getText());
      }
    }
    if (!Utilities.noString(anchor))
      tc.addTag("a").setAttribute("name", nmTokenize(anchor)).addText(" ");
    return tc;
  }


  private XhtmlNode addStyle(XhtmlNode node, Piece p) {
    if (p.getStyle() != null)
      node.setAttribute("style", p.getStyle());
    return node;
  }

  private String nmTokenize(String anchor) {
    return anchor.replace("[", "_").replace("]", "_");
  }


  private String srcFor(String filename) throws IOException {
    if (inLineGraphics) {
      StringBuilder b = new StringBuilder();
      b.append("data: image/png;base64,");
      byte[] bytes = FileUtils.readFileToByteArray(new File(Utilities.path(dest, filename)));
      b.append(new String(Base64.encodeBase64(bytes)));
      return b.toString();
    } else
      return filename;
  }


  private void checkModel(TableModel model) throws Exception {
    check(!model.getRows().isEmpty(), "Must have rows");
    check(!model.getTitles().isEmpty(), "Must have titles");
    for (Cell c : model.getTitles())
      check(c);
    for (Row r : model.getRows()) 
      check(r, "rows", model.getTitles().size());    
  }


  private void check(Cell c) throws Exception {  
    boolean hasText = false;
    for (Piece p : c.pieces)
      if (!Utilities.noString(p.getText()))
        hasText = true;
    check(hasText, "Title cells must have text");    
  }


  private void check(Row r, String string, int size) throws Exception {
    check(r.getCells().size() == size, "All rows must have the same number of columns as the titles");
    for (Row c : r.getSubRows()) 
      check(c, "rows", size);    
  }


  private String checkExists(List<Boolean> indents, boolean hasChildren) throws Exception {
    StringBuilder b = new StringBuilder();
    if (inLineGraphics) {
      ByteArrayOutputStream bytes = new ByteArrayOutputStream();
      genImage(indents, hasChildren, bytes);
      b.append("data: image/png;base64,");
      byte[] encodeBase64 = Base64.encodeBase64(bytes.toByteArray());
      b.append(new String(encodeBase64));
    } else {
      b.append("tbl_bck");
      for (Boolean i : indents)
        b.append(i ? "0" : "1");
      if (hasChildren)
        b.append("1");
      else
        b.append("0");
      b.append(".png");
      String file = Utilities.path(dest, b.toString());
      if (!new File(file).exists()) {
        FileOutputStream stream = new FileOutputStream(file);
        genImage(indents, hasChildren, stream);
      }
    }
    return b.toString();
  }


  private void genImage(List<Boolean> indents, boolean hasChildren, OutputStream stream) throws IOException {
    BufferedImage bi = new BufferedImage(400, 2, BufferedImage.TYPE_BYTE_BINARY);
    Graphics2D graphics = bi.createGraphics();
    graphics.setBackground(Color.WHITE);
    graphics.clearRect(0, 0, 600, 2);
    for (int i = 0; i < indents.size(); i++) {
      if (!indents.get(i))
        bi.setRGB(12+(i*16), 0, 0);
    }
    if (hasChildren)
      bi.setRGB(12+(indents.size()*16), 0, 0);
    ImageIO.write(bi, "PNG", stream);
  }

  private void check(boolean check, String message) throws Exception {
    if (!check)
      throw new Exception(message);
  }
}
#endif