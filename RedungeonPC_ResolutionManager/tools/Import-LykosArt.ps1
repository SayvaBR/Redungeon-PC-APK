param([Parameter(Mandatory=$true)][string]$Source, [string]$Extras)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
# Deterministic content import after ImageGen: isolate frames, preserve aspect,
# quantize to the game's small palette, align feet, and pack the runtime atlas.
Add-Type -ReferencedAssemblies System.Drawing.Common,System.Drawing.Primitives,System.Private.Windows.GdiPlus,System.Private.Windows.Core,System.Runtime,System.Collections -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
public static class LykosAtlasImport {
 static readonly int[] colors={0x111018,0x241B25,0x50252A,0x77331D,0xA14A24,0xC47132,0xEFA14D,0xB97556,0xDCA581,0xF6D6AF,0xFFF0D3,0x4D293D,0x823043,0xAF4053,0x303342,0x212430,0x151722,0x78717B,0xABA3A0,0xFFBF40,0xD4F9FF,0x1EE9F7,0x3370B8};
 static Bitmap Crop(Bitmap b,Rectangle r) => b.Clone(r,PixelFormat.Format32bppArgb);
 static Rectangle Bounds(Bitmap b) { int x0=b.Width,y0=b.Height,x1=-1,y1=-1; for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++)if(b.GetPixel(x,y).A>0){x0=Math.Min(x0,x);y0=Math.Min(y0,y);x1=Math.Max(x1,x);y1=Math.Max(y1,y);} if(x1<0)throw new Exception("Empty sprite"); return Rectangle.FromLTRB(x0,y0,x1+1,y1+1); }
 static Bitmap Resize(Bitmap b,int w,int h) { var r=new Bitmap(w,h,PixelFormat.Format32bppArgb); using(var g=Graphics.FromImage(r)){g.InterpolationMode=InterpolationMode.NearestNeighbor;g.PixelOffsetMode=PixelOffsetMode.Half;g.CompositingMode=CompositingMode.SourceCopy;g.DrawImage(b,new Rectangle(0,0,w,h),new Rectangle(0,0,b.Width,b.Height),GraphicsUnit.Pixel);} return r; }
 static void Palette(Bitmap b) {for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){var p=b.GetPixel(x,y);if(p.A<128){b.SetPixel(x,y,Color.Transparent);continue;}int best=0,dist=int.MaxValue; foreach(int c in colors){int dr=p.R-((c>>16)&255),dg=p.G-((c>>8)&255),db=p.B-(c&255);int d=dr*dr+dg*dg+db*db;if(d<dist){dist=d;best=c;}}b.SetPixel(x,y,Color.FromArgb(255,(best>>16)&255,(best>>8)&255,best&255));}}
 static void CleanFringe(Bitmap b) {
  var remove=new List<Point>();
  for(int y=0;y<b.Height;y++)for(int x=0;x<b.Width;x++){
   var p=b.GetPixel(x,y); if(p.A==0)continue;
   bool neutral=(p.R==120&&p.G==113&&p.B==123)||(p.R==171&&p.G==163&&p.B==160);
   if(!neutral)continue;
   int opaque=0; bool touchesTransparency=false;
   for(int yy=Math.Max(0,y-1);yy<=Math.Min(b.Height-1,y+1);yy++)for(int xx=Math.Max(0,x-1);xx<=Math.Min(b.Width-1,x+1);xx++)if(xx!=x||yy!=y){if(b.GetPixel(xx,yy).A>0)opaque++;else touchesTransparency=true;}
   // Generated checkerboard residue forms pale one-pixel whiskers along the
   // silhouette. Interior gray details have a dense neighborhood and remain.
   if(touchesTransparency || opaque<=4)remove.Add(new Point(x,y));
  }
  foreach(var p in remove)b.SetPixel(p.X,p.Y,Color.Transparent);
 }
 public static string Extras(string source,string output) {
  using(var raw=new Bitmap(source))using(var input=new Bitmap(raw.Width,raw.Height,PixelFormat.Format32bppArgb)) {
   using(var g=Graphics.FromImage(input))g.DrawImageUnscaled(raw,0,0);
   int w=input.Width,h=input.Height;var seen=new bool[w*h];var q=new Queue<int>();
   for(int x=0;x<w;x++){q.Enqueue(x);q.Enqueue((h-1)*w+x);}for(int y=0;y<h;y++){q.Enqueue(y*w);q.Enqueue(y*w+w-1);}
   while(q.Count>0){int i=q.Dequeue();if(seen[i])continue;seen[i]=true;int x=i%w,y=i/w;var c=input.GetPixel(x,y);int mx=Math.Max(c.R,Math.Max(c.G,c.B)),mn=Math.Min(c.R,Math.Min(c.G,c.B));if(c.A>0 && !(mn>145 && mx-mn<32))continue;input.SetPixel(x,y,Color.Transparent);if(x>0)q.Enqueue(i-1);if(x<w-1)q.Enqueue(i+1);if(y>0)q.Enqueue(i-w);if(y<h-1)q.Enqueue(i+w);}
   var entries=new List<string>();Bitmap atlas;using(var old=new Bitmap(output))atlas=new Bitmap(old);
   using(atlas)using(var g=Graphics.FromImage(atlas)){
    g.CompositingMode=CompositingMode.SourceCopy;
    string[] names={"lykos_portrait","lykos_wolf_portrait","lykos_bite","lykos_reinforce","lykos_zapped","lykos_wolf_zapped"};
    int[] tops={0,0,710,710,1030,1030},bottoms={710,710,1030,1030,1536,1536};
    int[] targetH={68,68,14,18,27,29},targetX={200,405,300,330,350,378},targetY={40,140,0,0,45,45};
    for(int i=0;i<6;i++){
     int left=(i%2)*w/2;
     using(var cell=Crop(input,new Rectangle(left,tops[i]*h/1536,w/2,(bottoms[i]-tops[i])*h/1536)))using(var b=Crop(cell,Bounds(cell)))using(var scaled=Resize(b,Math.Max(1,(int)Math.Round(b.Width*(double)targetH[i]/b.Height)),targetH[i])){
      Palette(scaled);g.FillRectangle(Brushes.Transparent,targetX[i],targetY[i],i<2?68:28,i<2?80:30);g.DrawImageUnscaled(scaled,targetX[i],targetY[i]);
      int lx=i<2?scaled.Width/2:0,ly=i<2?scaled.Height-2:0;
      entries.Add("\""+names[i]+"\":["+targetX[i]+","+targetY[i]+","+scaled.Width+","+scaled.Height+","+lx+","+ly+"]");
     }
    }
    atlas.Save(output,ImageFormat.Png);
   }
   return "{"+string.Join(",",entries)+"}";
  }
 }
 public static string Build(string source,string output) {
  using(var raw=new Bitmap(source)) using(var input=new Bitmap(raw.Width,raw.Height,PixelFormat.Format32bppArgb)) {
   using(var g=Graphics.FromImage(input))g.DrawImageUnscaled(raw,0,0);
   // Some generators bake a preview checkerboard into RGB. Remove only the
   // connected neutral backdrop; enclosed eyes/teeth and dark outlines remain.
   int w=input.Width,h=input.Height; var visited=new bool[w*h];var q=new Queue<int>();
   for(int x=0;x<w;x++){q.Enqueue(x);q.Enqueue((h-1)*w+x);}for(int y=0;y<h;y++){q.Enqueue(y*w);q.Enqueue(y*w+w-1);}
   while(q.Count>0){int i=q.Dequeue();if(visited[i])continue;visited[i]=true;int x=i%w,y=i/w;var c=input.GetPixel(x,y);int mx=Math.Max(c.R,Math.Max(c.G,c.B)),mn=Math.Min(c.R,Math.Min(c.G,c.B));if(c.A>0 && !(mn>145 && mx-mn<32))continue;input.SetPixel(x,y,Color.Transparent);if(x>0)q.Enqueue(i-1);if(x<w-1)q.Enqueue(i+1);if(y>0)q.Enqueue(i-w);if(y<h-1)q.Enqueue(i+w);}
   // Measured boundaries of the approved 1086 x 1448 generated sheet.
   int[] xs={15,198,372,546,723,877,1086};int[] ys={16,211,387,561,736,910,1089,1254,1448};
   var cells=new Bitmap[8,6];int maxHeight=0;
   for(int row=0;row<8;row++)for(int col=0;col<6;col++){
    int l=(int)Math.Round(xs[col]*w/1086.0),r=(int)Math.Round(xs[col+1]*w/1086.0),t=(int)Math.Round(ys[row]*h/1448.0),bt=(int)Math.Round(ys[row+1]*h/1448.0);
    using(var cell=Crop(input,Rectangle.FromLTRB(l,t,r,bt)))cells[row,col]=Crop(cell,Bounds(cell));
    if(col<4)maxHeight=Math.Max(maxHeight,cells[row,col].Height);
   }
   double scale=25.0/maxHeight;
   var normalized=new Bitmap[8,6];
   using(var atlas=new Bitmap(512,512,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(atlas)){
    g.CompositingMode=CompositingMode.SourceCopy;
    var entries=new List<string>();
    Action<string,Bitmap,int,int,int,int> put=(name,b,x,y,lx,ly)=>{g.DrawImageUnscaled(b,x,y);entries.Add("\""+name+"\":["+x+","+y+","+b.Width+","+b.Height+","+lx+","+ly+"]");};
    string[] dirs={"s","n","e","w","s","n","w","e"};
    for(int row=0;row<8;row++)for(int col=0;col<6;col++){
     var b=cells[row,col];using(var scaled=Resize(b,Math.Max(1,(int)Math.Round(b.Width*scale)),Math.Max(1,(int)Math.Round(b.Height*scale)))){
      Palette(scaled);CleanFringe(scaled);var frame=new Bitmap(32,32,PixelFormat.Format32bppArgb);using(var fg=Graphics.FromImage(frame))fg.DrawImageUnscaled(scaled,(32-scaled.Width)/2,29-scaled.Height);
      normalized[row,col]=frame;string name="lykos_"+(row<4?"human":"wolf")+"_"+dirs[row]+"_"+(col<4?(col+1).ToString():"death_"+(col-3));put(name,frame,col*32,row*32,0,0);
     } b.Dispose();
    }
    using(var icon=Crop(normalized[0,0],Bounds(normalized[0,0]))) {
     put("lykos_icon",icon,200,0,0,0);put("lykos_shot",icon,230,0,0,0);
     using(var portrait=Resize(icon,icon.Width*3,icon.Height*3))put("lykos_portrait",portrait,200,40,portrait.Width/2,portrait.Height-2);
    }
    using(var whole=Crop(normalized[4,0],Bounds(normalized[4,0])))using(var head=Crop(whole,new Rectangle(0,0,whole.Width,Math.Min(15,whole.Height)))){
     put("lykos_moon",head,270,0,0,0);put("lykos_bite",head,300,0,0,0);put("lykos_reinforce",head,330,0,0,0);
     using(var skull=Resize(head,head.Width*3,head.Height*3)){
      for(int y=0;y<skull.Height;y++)for(int x=0;x<skull.Width;x++){var p=skull.GetPixel(x,y);if(p.A>0){int gray=(p.R+p.G+p.B)/3;skull.SetPixel(x,y,gray<45?Color.FromArgb(255,33,26,40):Color.FromArgb(255,Math.Min(255,gray+100),Math.Min(255,gray+90),Math.Min(255,gray+72)));}}
     put("lykos_skull",skull,275,45,skull.Width/2,skull.Height*2/3);
     }
    }
    using(var moon=new Bitmap(25,25,PixelFormat.Format32bppArgb)){
     for(int y=0;y<25;y++)for(int x=0;x<25;x++){
      int dx=x-12,dy=y-12,d2=dx*dx+dy*dy;if(d2<=132){
       Color c=d2>112?Color.FromArgb(255,198,183,148):Color.FromArgb(255,246,230,180);
       if((x-8)*(x-8)+(y-8)*(y-8)<7||(x-16)*(x-16)+(y-14)*(y-14)<9||(x-10)*(x-10)+(y-17)*(y-17)<4)c=Color.FromArgb(255,215,198,153);
       moon.SetPixel(x,y,c);
      }
     }
     put("lykos_full_moon",moon,430,0,12,12);
    }
    for(int form=0;form<2;form++)using(var zap=Crop(normalized[form*4,0],Bounds(normalized[form*4,0]))) {
     for(int y=0;y<zap.Height;y++)for(int x=0;x<zap.Width;x++){var p=zap.GetPixel(x,y);if(p.A>0)zap.SetPixel(x,y,p.R+p.G+p.B<140?Color.FromArgb(255,47,110,179):Color.FromArgb(255,206,247,255));}
     put(form==0?"lykos_zapped":"lykos_wolf_zapped",zap,350+form*28,45,0,0);
    }
    for(int i=0;i<5;i++)using(var revival=new Bitmap(102,100,PixelFormat.Format32bppArgb))using(var rg=Graphics.FromImage(revival)){
     int col=i==0?5:i==1?4:0;using(var b=Crop(normalized[0,col],Bounds(normalized[0,col])))using(var large=Resize(b,b.Width*2,b.Height*2))rg.DrawImageUnscaled(large,(102-large.Width)/2,65-large.Height);
     put("lykos_revive_"+(i+1),revival,(i%4)*110,270+(i/4)*105,0,0);
    }
    atlas.Save(output,ImageFormat.Png);
    foreach(var b in normalized)b.Dispose();
    return "{\n"+string.Join(",\n",entries)+"\n}";
   }
  }
 }
}
'@
$root = Split-Path $PSScriptRoot -Parent
$png = Join-Path $root 'Content/Images/lykos.png'
$json = [LykosAtlasImport]::Build((Resolve-Path -LiteralPath $Source).Path,$png)
$entries = $json | ConvertFrom-Json -AsHashtable
if($Extras) {
    $extraEntries = [LykosAtlasImport]::Extras((Resolve-Path -LiteralPath $Extras).Path,$png) | ConvertFrom-Json -AsHashtable
    foreach($key in $extraEntries.Keys) { $entries[$key]=$extraEntries[$key] }
}
# Use the actual game's bold glyphs for the name badge, not a substitute font.
$fontPath = Join-Path (Split-Path $root -Parent) 'Sprites Redungeon/UI/Fonts/font_bold.png'
$font = [Drawing.Bitmap]::FromFile($fontPath)
$fonts = Get-Content (Join-Path $root 'Content/Fonts/fonts.json') -Raw | ConvertFrom-Json
$glyphs = ($fonts.fonts | Where-Object { $_.'sprite-name' -eq 'font_bold' }).glyphs
$loaded = [Drawing.Bitmap]::FromFile($png)
$atlas = [Drawing.Bitmap]::new($loaded)
$loaded.Dispose()
$cursor = 200
foreach($letter in 'LYKOS'.ToCharArray()) {
    $glyph = $glyphs | Where-Object { $_.glyph -ceq [string]$letter }
    for($y=0;$y -lt $glyph.h;$y++) { for($x=0;$x -lt $glyph.w;$x++) {
        $pixel=$font.GetPixel($glyph.x+$x,$glyph.y+$y)
        if($pixel.A -gt 0) { $atlas.SetPixel($cursor+$x,130+$y,[Drawing.Color]::FromArgb($pixel.A,250,181,63)) }
    }}
    $cursor += $glyph.w + 1
}
$entries['lykos_name'] = @(200,130,($cursor-201),11,0,0)
$atlas.Save($png,[Drawing.Imaging.ImageFormat]::Png)
$atlas.Dispose(); $font.Dispose()
[IO.File]::WriteAllText((Join-Path $root 'Content/Images/lykos.json'),($entries | ConvertTo-Json -Depth 4))
Write-Host "Lykos atlas: $png"
