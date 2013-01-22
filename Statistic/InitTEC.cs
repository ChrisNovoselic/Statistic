using System;
using System.Collections.Generic;
using System.Text;

namespace Statistic
{
    public class InitTEC
    {
        public List<TEC> tec;

        public InitTEC()
        {
            // ๏๎ไ๊๋þ๗่๒ü๑ÿ ๊ แไ, ่ํ่๖่เ๋่็่๐๎โเ๒ü ใ๋๎แเ๋üํ๛ๅ ๏ๅ๐ๅ์ๅํํ๛ๅ, โ๛แ๐เ๒ü ๐ๅๆ่์ ๐เแ๎๒๛

            int i, j, k; //ศํไๅ๊๑๛ ไ๋ÿ าÝึ, ราฯ, าร
            tec = new List<TEC>();

            i = j = k = 0;
            tec.Add (new TEC ());
            tec [i].name = "มาÝึ";
            
            tec [i].GTP.Add (new GTP (tec [i]));
            tec [i].GTP [j].name = "";
            tec [i].GTP [j].TG.Add (new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [ k++].name = "าร1";
            tec [i].GTP [j].TG.Add(new TG (tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "าร2";
            tec [i].GTP [j].TG.Add (new TG(tec [i].GTP [j]));
            tec [i].GTP [j].TG [k ++].name = "าร3";
            tec [i].GTP [j].TG.Add (new TG(tec[i].GTP[j]));
            tec [i].GTP [j].TG [k ++].name = "าร4";
            tec [i].GTP [j].TG.Add(new TG(tec[i].GTP[j]));
            tec [i].GTP [j].TG [k ++].name = "าร5";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "าÝึ-2";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร9";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "าÝึ-3";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ราฯ 110 ๊ย";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร8";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร9";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร10";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร11";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "าร12";
            k = 0;
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ราฯ 220 ๊ย";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร13";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร14";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "าÝึ-4";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร6";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร7";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร8";

            j = k = 0;
            i++;
            tec.Add(new TEC());
            tec[i].name = "าÝึ-5";
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ราฯ 220 ๊ย";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร1";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j++].TG[k++].name = "าร2";
            k = 0;
            tec[i].GTP.Add(new GTP(tec[i]));
            tec[i].GTP[j].name = "ราฯ 110 ๊ย";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร3";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร4";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร5";
            tec[i].GTP[j].TG.Add(new TG(tec[i].GTP[j]));
            tec[i].GTP[j].TG[k++].name = "าร6";
        }
    }
}
