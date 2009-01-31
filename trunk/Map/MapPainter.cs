using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace EasyTravian
{
    /// <summary>
    /// Térkép megjelítésével foglalkozik
    /// </summary>
    class MapPainter
    {
        private Point LastMapInfoPoint;
        private MapElement LastMapInfoElement = null;

        /// <summary>
        /// Kirajzolja a térképet
        /// </summary>
        /// <param name="graphics">Ide fogja rajzolni</param>
        /// <param name="map">ezt a térképet</param>
        /// <param name="origin">ez a középpontja</param>
        /// <param name="zoom">zoom mértéke</param>
        public void Paint(Graphics graphics, Dictionary<Point, MapElement> map, Dictionary<Point, TerrainType> Terrain, Point origin, MapPainterProps props)
        {
            if (map == null)
                return;

            int xd = (int)graphics.VisibleClipBounds.Width / (props.zoom * 2 + 1);
            int yd = (int)graphics.VisibleClipBounds.Height / (props.zoom * 2 + 1);

            xd = yd = System.Math.Min(xd, yd);

            Point actPos = new Point();
            MapElement me = new MapElement();
            Rectangle rect = new Rectangle();
            rect.Width = xd;
            rect.Height = yd;
            Pen pen = new Pen(Color.Red);
            Font font = new Font(FontFamily.GenericSerif, 10);
            Color fillColor = Color.Empty;
            SolidBrush brush = new SolidBrush(Color.Red);

            int MaxPopulation = 0;

            if (props.Coloring == MapColoring.Population)
                MaxPopulation = map.Max(m => m.Value.Population);

            TerrainType tt = TerrainType.NA;

            for (int x = -props.zoom; x <= props.zoom; x++)
            {
                for (int y = -props.zoom; y <= props.zoom; y++)
                {
                    actPos.X = origin.X + x;
                    actPos.Y = origin.Y + y;

                    map.TryGetValue(actPos, out me);

                    rect.X = (x + props.zoom) * xd;
                    rect.Y = (-y + props.zoom) * yd;

                    if (me != null)
                    {
                        fillColor = Color.Empty;

                        if (props.Coloring == MapColoring.Population)
                        {
                            fillColor = Color.FromArgb(me.Population * 255 / MaxPopulation, Color.Red);
                        }

                        if (props.Coloring == MapColoring.Ally)
                        {
                            props.Alliances.TryGetValue(me.Alliance, out fillColor);
                        }

                        if (props.Coloring == MapColoring.Tribe)
                        {
                            switch (me.Tid)
                            {
                                case 1: 
                                    fillColor = Color.Red;
                                    break;
                                case 2:
                                    fillColor = Color.Blue;
                                    break;
                                case 3:
                                    fillColor = Color.Green;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if (fillColor == Color.Empty)
                            continue;

                        brush.Color = fillColor;

                        if (props.zoom <= 10)
                        {
                            graphics.FillRectangle(brush, rect);
                            //graphics.DrawRectangle(pen, rect);
                            graphics.DrawString(me.Village + "\n" + me.Player + "\n" + me.Alliance, font, Brushes.Black, rect.Location);
                        }
                        if (props.zoom <= 100 && props.zoom > 10)
                        {
                            graphics.FillRectangle(brush, rect);
                        }
                        if (props.zoom > 100)
                        {
                            graphics.FillRectangle(brush, rect);
                            //putpixel? mindenképp vmi gyorsabb
                        }
                        //graphics.DrawEllipse(pen, rect);
                        //graphics.DrawRectangle(pen, rect);
                    }

                    if (Terrain.TryGetValue(actPos, out tt))
                    {
                        graphics.DrawEllipse(pen, rect);
                    }

                }
            }    
        }

        /// <summary>
        /// Egérpozícióból kitalálja a pointer alatti falut
        /// </summary>
        /// <param name="size">Ez a kirajzolt térkép mérete</param>
        /// <param name="point">Itt az egér</param>
        /// <param name="origin">Ez a falu van a középpontban</param>
        /// <param name="map">Térképke</param>
        /// <param name="zoom">zoom</param>
        /// <returns></returns>
        internal MapElement GetMapInfo(Size size, Point point, Point origin, Dictionary<Point, MapElement> map, int zoom)
        {
            if (map == null)
                return null;

            int xd = (int)size.Width / (zoom * 2 + 1);
            int yd = (int)size.Height / (zoom * 2 + 1);

            xd = yd = System.Math.Min(xd, yd);

            Point actPos = new Point(point.X / xd + origin.X - zoom, origin.Y - (point.Y / yd) + zoom);

            if (LastMapInfoPoint == actPos)
                return LastMapInfoElement;

            MapElement me = null;

            map.TryGetValue(actPos, out me);

            //Ez a szarakodás azért kell, hogy bizonyoz zoom alatt
            //a terraint meg lehessen jeleniteni
            if (me == null && zoom < 30)
            {
                me = new MapElement();
                me.X = actPos.X;
                me.Y = actPos.Y;
            }

            LastMapInfoPoint = actPos;
            LastMapInfoElement = me;

            return me;
        }
    }

    public enum MapColoring {Population, Activity, Clans, Ally, Tribe};

    /// <summary>
    /// Térkép megrajzolásának beállításai
    /// </summary>
    public class MapPainterProps
    {
        public int zoom;
        public MapColoring Coloring = MapColoring.Tribe;
        public SerializableDictionary<string, Color> Alliances = new SerializableDictionary<string, Color>();

    }
}
