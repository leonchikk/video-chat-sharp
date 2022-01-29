using Multimedia.Video.Desktop.AVI.RIFF;
using Multimedia.Video.Desktop.AVI.Structures;
using System;
using System.Collections.Generic;
using System.IO;

namespace Multimedia.Video.Desktop.AVI
{
    //TODO: Translate japanese comments
    public class AviWriter
    {
        private event Action<byte[], bool> OnAddImage;
        public void AddImage(byte[] data, bool keyFrame) { OnAddImage(data, keyFrame); }

        private event Action OnClose;
        public void Close() { OnClose(); }

        internal RiffFile Riff { get; set; }

        public AviWriter(Stream outputAvi, string fourCC, int width, int height, float fps)
        {
            Create(outputAvi, fourCC, width, height, fps);
        }

        public void Create(Stream outputAvi, string fourCC, int width, int height, float fps)
        {
            Riff = new RiffFile(outputAvi, "AVI ");
            var hdrlList = Riff.CreateList("hdrl");
            WriteHdrlList(hdrlList, fourCC, width, height, fps, 0);
            hdrlList.Close();

            // moviリストを作成し、OnAddImageごとにデータチャンクを追加
            var idx1List = new List<Idx1Entry>();
            var moviList = Riff.CreateList("movi");
            this.OnAddImage += (data, keyFrame) =>
            {
                var idx1 = WriteMoviList(moviList, "00dc", data);
                idx1.KeyFrame = keyFrame;
                idx1List.Add(idx1);
            };
        }

        private void WriteHdrlList(RiffList hdrlList, string fourCC, int width, int height, float fps, int frames)
        {
            int streams = 1; // ストリーム数。音声なしの場合1。ありの場合2。

            // LISTチャンク'hdrl'を追加

            // 'hdrl' リストは AVI メイン ヘッダーで始まり、このメイン ヘッダーは 'avih' チャンクに含まれている。
            // メイン ヘッダーには、ファイル内のストリーム数、AVI シーケンスの幅と高さなど、AVI ファイル全体に関するグローバル情報が含まれる。
            // メイン ヘッダー チャンクは、AVIMAINHEADER 構造体で構成されている。
            {
                var chunk = hdrlList.CreateChunk("avih");
                var avih = new AVIMAINHEADER();
                avih.dwMicroSecPerFrame = (uint)(1 / fps * 1000 * 1000);
                avih.dwMaxBytesPerSec = 25000; // ffmpegと同じ値に
                avih.dwFlags = 0x0910;         // ffmpegと同じ値に
                avih.dwTotalFrames = (uint)frames;
                avih.dwStreams = (uint)streams;
                avih.dwSuggestedBufferSize = 0x100000;
                avih.dwWidth = (uint)width;
                avih.dwHeight = (uint)height;

                var data = StructureToBytes(avih);
                chunk.Write(data);
                chunk.Close();
            }

            // メイン ヘッダーの次には、1 つ以上の 'strl' リストが続く。'strl' リストは各データ ストリームごとに必要である。
            // 各 'strl' リストには、ファイル内の単一のストリームに関する情報が含まれ、ストリーム ヘッダー チャンク ('strh') とストリーム フォーマット チャンク ('strf') が必ず含まれる。
            // ストリーム ヘッダー チャンク ('strh') は、AVISTREAMHEADER 構造体で構成されている。
            // ストリーム フォーマット チャンク ('strf') は、ストリーム ヘッダー チャンクの後に続けて記述する必要がある。
            // ストリーム フォーマット チャンクは、ストリーム内のデータのフォーマットを記述する。このチャンクに含まれるデータは、ストリーム タイプによって異なる。
            // ビデオ ストリームの場合、この情報は必要に応じてパレット情報を含む BITMAPINFO 構造体である。オーディオ ストリームの場合、この情報は WAVEFORMATEX 構造体である。

            // Videoｽﾄﾘｰﾑ用の'strl'チャンク
            var strl_list = hdrlList.CreateList("strl");
            {
                var chunk = strl_list.CreateChunk("strh");
                var strh = new AVISTREAMHEADER();
                strh.fccType = ToFourCC("vids");
                strh.fccHandler = ToFourCC(fourCC);
                strh.dwScale = 1000 * 1000; // fps = dwRate / dwScale。秒間30フレームであることをあらわすのにdwScale=33333、dwRate=1000000という場合もあればdwScale=1、dwRate=30という場合もあります
                strh.dwRate = (int)(fps * strh.dwScale);
                strh.dwLength = frames;
                strh.dwSuggestedBufferSize = 0x100000;
                strh.dwQuality = -1;

                var data = StructureToBytes(strh);
                chunk.Write(data);
                chunk.Close();
            }
            {
                var chunk = strl_list.CreateChunk("strf");
                var strf = new BITMAPINFOHEADER();
                strf.biWidth = width;
                strf.biHeight = height;
                strf.biBitCount = 24;
                strf.biSizeImage = strf.biHeight * ((3 * strf.biWidth + 3) / 4) * 4; // らしい
                strf.biCompression = ToFourCC(fourCC);
                strf.biSize = System.Runtime.InteropServices.Marshal.SizeOf(strf);
                strf.biPlanes = 1;

                var data = StructureToBytes(strf);
                chunk.Write(data);
                chunk.Close();
            }
            strl_list.Close();
        }

        // たとえば、ストリーム 0 にオーディオが含まれる場合、そのストリームのデータ チャンクは FOURCC '00wb' を持つ。
        // ストリーム 1 にビデオが含まれる場合、そのストリームのデータ チャンクは FOURCC '01db' または '01dc' を持つ。
        private static Idx1Entry WriteMoviList(RiffList moviList, string chunkId, byte[] data)
        {
            var chunk = moviList.CreateChunk(chunkId);
            chunk.Write(data);

            // データはワード境界に配置しなければならない
            // バイト数が奇数の場合は、1バイトのダミーデータを書き込んでワード境界にあわせる
            int length = data.Length;
            bool padding = false;
            if (length % 2 != 0)
            {
                chunk.WriteByte(0x00); // 1バイトのダミーを書いてワード境界にあわせる
                padding = true;
            }

            chunk.Close();

            return new Idx1Entry(chunkId, length, padding);
        }

        private static int ToFourCC(string fourCC)
        {
            if (fourCC.Length != 4) throw new ArgumentException("must be 4 characters long.", "fourCC");
            return ((int)fourCC[3]) << 24 | ((int)fourCC[2]) << 16 | ((int)fourCC[1]) << 8 | ((int)fourCC[0]);
        }


        private static byte[] StructureToBytes<T>(T st) where T : struct
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(st);
            IntPtr ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            System.Runtime.InteropServices.Marshal.StructureToPtr(st, ptr, false);

            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, size);

            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
            return data;
        }
    }
}
