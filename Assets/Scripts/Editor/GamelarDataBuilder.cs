using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamelAR.EditorTools
{
    /// <summary>Membuat 15 aset InstrumentData + InstrumentDatabase (aman dijalankan berulang).</summary>
    public static class GamelarDataBuilder
    {
        const string DataDir = "Assets/ScriptableObjects/Instruments";
        const string DbPath = "Assets/Resources/InstrumentDatabase.asset";

        struct Info
        {
            public string id, nama, tagline, kelompok, fungsi, teknik, sejarah;
            public Info(string id, string nama, string tagline, string kelompok, string fungsi, string teknik, string sejarah)
            { this.id = id; this.nama = nama; this.tagline = tagline; this.kelompok = kelompok; this.fungsi = fungsi; this.teknik = teknik; this.sejarah = sejarah; }
        }

        static readonly Info[] All =
        {
            new Info("BonangBarung", "Bonang Barung", "Penuntun melodi utama gamelan", "Pencon",
                "Memimpin jalannya lagu pada gendhing <b>soran</b> (bergaya keras) dan memainkan <b>buka</b> (lagu pembuka) pada gendhing bonang. Pada gendhing lain, bonang barung menghias dan mendahului balungan.",
                "Dimainkan dengan dua tabuh kayu yang dililit tali, satu di tiap tangan, dipukulkan ke <b>pencu</b> (tonjolan di tengah pencon). Teknik utamanya <b>mipil</b> (memukul nada bergantian, mendahului nada balungan), <b>gembyang</b> (memukul dua nada berjarak satu oktaf bersamaan), dan <b>imbal</b> (saling mengisi dengan bonang penerus). Pada gendhing bonang, penabuh bonang barung juga memainkan <b>buka</b> (lagu pembuka).",
                "Instrumen pencon berderet seperti bonang diperkirakan sudah dipakai di Jawa sejak akhir masa Hindu-Buddha dan terus berkembang pada masa Islam. Menurut cerita tutur, nama bonang kerap dikaitkan dengan Sunan Bonang, salah satu Wali Sanga yang memakai gamelan sebagai sarana dakwah. Kini bonang barung menjadi bagian tetap gamelan ageng, termasuk di keraton Surakarta dan Yogyakarta."),
            new Info("BonangPenerus", "Bonang Penerus", "Penghias melodi satu oktaf lebih tinggi", "Pencon",
                "Mengisi dan menghias melodi dengan pola yang lebih rapat, satu oktaf di atas bonang barung.",
                "Ditabuh dengan dua tabuh berlilit tali seperti bonang barung, tetapi dengan pukulan dua kali lebih rapat. Teknik utamanya <b>mipil</b> cepat, <b>imbal</b> bersama bonang barung, dan <b>sekaran</b> (variasi hiasan) menjelang akhir gatra. Berbeda dengan bonang barung, bonang penerus tidak memainkan buka.",
                "Kata <i>penerus</i> dipakai untuk instrumen yang bernada satu oktaf lebih tinggi daripada pasangannya, seperti gender penerus dan saron penerus. Bonang penerus berkembang sebagai pasangan bonang barung dalam gamelan ageng Jawa; keduanya membentuk jalinan imbal yang menjadi ciri khas permainan bonang."),
            new Info("Demung", "Demung", "Pembawa kerangka lagu (balungan)", "Balungan",
                "Memainkan <b>balungan</b>, yaitu kerangka melodi yang menjadi dasar bagi instrumen lain.",
                "Bilah dipukul dengan tabuh kayu berbentuk palu di tangan kanan. Tangan kiri <b>methet</b>, yaitu memegang bilah yang baru dibunyikan tepat saat bilah berikutnya dipukul, agar nada tidak saling bertumpuk. Pola permainannya antara lain mengikuti balungan nada demi nada (<b>mlaku</b>), memukul lebih jarang (<b>nibani</b>), atau <b>imbal</b> dengan demung kedua.",
                "Demung adalah anggota keluarga saron yang paling besar dan bernada paling rendah. Bilahnya umumnya ditempa dari perunggu, yang dalam bahasa Jawa disebut <i>gangsa</i>. Dalam satu perangkat gamelan ageng biasanya terdapat satu atau dua demung untuk tiap laras, pelog dan slendro."),
            new Info("SaronBarung", "Saron Barung", "Balungan satu oktaf di atas demung", "Balungan",
                "Memainkan balungan dengan nada satu oktaf di atas demung dan memberi ketukan melodi yang jelas.",
                "Dipukul dengan tabuh kayu berbentuk palu di tangan kanan, sementara tangan kiri <b>methet</b> (meredam bilah sebelumnya). Saron barung memainkan balungan secara <b>mlaku</b> atau <b>nibani</b>; pada gendhing tertentu dua saron bermain <b>imbal</b> sehingga terdengar seperti satu melodi yang lebih rapat. Bilahnya diletakkan di atas rancakan kayu berongga yang menjadi ruang resonansi.",
                "Saron merupakan salah satu instrumen pokok gamelan Jawa; instrumen bilah logam sejenis juga dikenal dalam gamelan Bali dengan nama <i>gangsa</i>. Dalam gamelan ageng biasanya terdapat dua saron barung atau lebih untuk tiap laras, bersama <b>saron penerus</b> (peking) yang bernada satu oktaf lebih tinggi."),
            new Info("Slenthem", "Slenthem", "Balungan bernada paling rendah", "Balungan",
                "Memainkan balungan dengan nada paling rendah dan memberi dasar yang tenang bagi ensambel.",
                "Dimainkan dengan satu tabuh bertangkai yang ujungnya berupa piringan berbalut kain, dipegang tangan kanan; tangan kiri meredam bilah sebelumnya. Slenthem biasanya memainkan balungan dengan pukulan tenang atau <b>nibani</b> (lebih jarang), sehingga dengung panjangnya terdengar jelas di lapisan nada paling bawah.",
                "Bentuk slenthem serupa gender, yaitu bilah logam yang digantung di atas tabung resonator, tetapi bilahnya lebih lebar dan hanya mencakup satu oktaf. Karena kemiripan itu, slenthem di beberapa tempat juga disebut <i>gender panembung</i>."),
            new Info("GenderBarung", "Gender Barung", "Penghias melodi bernuansa lembut", "Penghias melodi",
                "Mengiringi dan menghias lagu dengan pola cengkok, terutama pada gamelan bernuansa lembut.",
                "Dua tabuh berbentuk piringan berbantalan dipegang di kedua tangan. Sambil memukul nada baru, sisi tangan yang sama meredam nada sebelumnya, sehingga setiap tangan memukul dan meredam sekaligus. Tangan kanan dan kiri memainkan dua alur melodi yang berbeda, disebut <b>cengkok</b> atau <b>genderan</b>; karena itu gender termasuk instrumen yang paling sulit dimainkan.",
                "Gender dikenal dalam dua ukuran: gender barung dan gender penerus yang bernada satu oktaf lebih tinggi. Gender menjadi instrumen utama pengiring dalang dalam pertunjukan wayang kulit, terutama untuk mengiringi <i>suluk</i> (nyanyian dalang)."),
            new Info("Gambang", "Gambang", "Bilah kayu penghias melodi", "Penghias melodi",
                "Memainkan pola melodi yang mengalir, sering dalam gembyang (oktaf), untuk memperkaya lagu.",
                "Dua tabuh panjang dan lentur, biasanya dari tanduk, dengan ujung bulat berbantalan dipegang di kedua tangan. Kedua tangan umumnya memukul nada yang sama berjarak satu oktaf (<b>gembyangan</b>) dengan tempo cepat dan mengalir, mengisi sela-sela balungan dengan rangkaian nada yang rapat.",
                "Gambang adalah satu-satunya instrumen bilah kayu dalam gamelan ageng Jawa. Bilahnya, sekitar 18-23 buah, dibuat dari kayu keras dan diletakkan di atas kotak kayu yang berfungsi sebagai resonator. Alat musik bilah kayu sejenis gambang dikenal luas di Asia Tenggara, dan di Jakarta menjadi bagian ansambel Betawi <i>gambang kromong</i>."),
            new Info("GongAgeng", "Gong Ageng", "Penanda akhir bagian besar lagu", "Struktural",
                "Menandai akhir satu putaran lagu (gongan) dengan bunyi yang dalam dan panjang.",
                "Ditabuh dengan tabuh besar berkepala bulat berbalut kain tebal pada <b>pencu</b> (tonjolan di tengah). Pukulan dilakukan lembut tetapi mantap agar gong berdengung panjang dengan getaran naik-turun yang disebut <b>ombak</b>. Gong dibunyikan pada akhir setiap gongan, sehingga penabuhnya harus mengikuti struktur lagu dengan teliti.",
                "Menurut legenda Jawa, gong adalah instrumen gamelan pertama, diciptakan oleh Sang Hyang Guru untuk memanggil para dewa; instrumen lain menyusul kemudian. Gong ageng ditempa dari perunggu oleh pande (pandai logam) dan pembuatannya membutuhkan keahlian tinggi, sehingga gong sering dianggap pusaka, diberi nama kehormatan seperti <i>Kyai</i>, dan pada tradisi tertentu diberi sesaji."),
            new Info("Kempul", "Kempul", "Penanda frase di dalam gongan", "Struktural",
                "Menandai bagian-bagian di dalam satu gongan dan memberi tekanan pada frase lagu.",
                "Ditabuh dengan tabuh berbantalan pada pencu dan dibiarkan berdengung. Kempul terutama dipakai pada bentuk lagu yang lebih pendek seperti lancaran, ketawang, dan ladrang. Nada kempul yang dipukul dipilih penabuh agar selaras dengan nada balungan pada saat itu.",
                "Kempul termasuk keluarga gong gantung. Satu perangkat gamelan biasanya memiliki beberapa kempul dengan nada berbeda yang digantung berderet pada <b>gayor</b> (rangka gantungan) bersama gong ageng dan gong suwukan."),
            new Info("Kenong", "Kenong", "Pembagi frase lagu", "Struktural",
                "Menandai pembagian frase lagu dengan nada yang jelas dan berukuran sedang.",
                "Dipukul dengan tabuh berlilit tali pada pencu lalu dibiarkan berdengung. Kenong menandai pembagian gongan menjadi beberapa bagian yang disebut <b>kenongan</b>; misalnya pada ladrang, satu gongan dibagi menjadi empat kenongan. Penabuh memilih pencon kenong yang nadanya sesuai alur lagu.",
                "Nama kenong diduga berasal dari bunyinya (<i>nong</i>). Bersama kethuk, kempul, dan gong, kenong termasuk instrumen <b>kolotomik</b>, yaitu penanda struktur yang memberi bentuk pada gendhing Jawa. Kenong diletakkan mendatar di atas tali yang direntangkan pada rancakan kayu."),
            new Info("Kethuk", "Kethuk", "Penanda ketukan bernada pendek", "Struktural",
                "Memberi ketukan pendek yang menandai pembagian waktu, sering berpasangan dengan kenong.",
                "Dipukul dengan tabuh berlilit tali, lalu tabuh ditahan sebentar menempel pada pencu sehingga bunyinya pendek dan tertahan: <i>thuk</i>. Kethuk berbunyi di sela-sela pukulan kenong sebagai penanda ketukan di dalam bagian lagu.",
                "Nama kethuk berasal dari bunyinya yang pendek. Kethuk sering dipasangkan dengan <b>kempyang</b>, gong kecil bernada tinggi, dan keduanya diletakkan berdampingan pada satu rancakan. Sebagai instrumen kolotomik, kethuk ikut menentukan nama bentuk gendhing, misalnya <i>kethuk 2 kerep</i> atau <i>kethuk 4 awis</i>."),
            new Info("Kendhang", "Kendhang", "Pemimpin irama gamelan", "Pemimpin irama",
                "Mengatur tempo, peralihan, dan dinamika sehingga dikenal sebagai <b>pemimpin irama</b>.",
                "Kendhang diletakkan melintang di atas <b>plangkan</b> (penyangga) dan dipukul dengan telapak tangan serta jari pada kedua sisinya. Letak dan cara pukulan yang berbeda menghasilkan bunyi dasar seperti <i>dhah</i>, <i>dhung</i>, <i>tak</i>, <i>thung</i>, dan <i>ket</i>. Rangkaian bunyi itu menjadi aba-aba bagi seluruh penabuh: kapan mulai, mempercepat, memperlambat, berganti irama, atau berhenti (<i>suwuk</i>).",
                "Gendang berkepala dua sudah tergambar pada relief Candi Borobudur (abad ke-8). Badan kendhang dibuat dari kayu yang dilubangi, misalnya nangka atau cempedak, dan kedua sisinya ditutup kulit kerbau atau kambing. Dalam karawitan Jawa dikenal beberapa ukuran, antara lain kendhang ageng, kendhang ciblon, dan ketipung."),
            new Info("Rebab", "Rebab", "Penuntun lagu bersuara merdu", "Gesek",
                "Menuntun lagu pada gamelan lembut dengan melantunkan melodi pembuka yang kemudian diikuti instrumen lain.",
                "Rebab dimainkan sambil duduk bersila dengan badan rebab ditegakkan di depan pemain. Busur (<b>kosok</b>) digesekkan pada dua dawai, sementara jari tangan kiri menekan dawai tanpa menempel ke leher (tidak ada papan nada), sehingga nada bisa digeser dan dihias dengan luwes. Pada gendhing rebab, pemain rebab memainkan <b>buka</b> yang menjadi aba-aba awal lagu.",
                "Kata rebab berasal dari bahasa Arab <i>rabab</i>. Alat gesek ini diperkirakan masuk ke Nusantara bersama para pedagang dari Timur Tengah sekitar masa penyebaran Islam, lalu berkembang menjadi rebab Jawa dengan badan (<i>bathokan</i>) dari tempurung atau kayu yang dilapisi kulit tipis. Dalam karawitan Jawa, rebab dikenal sebagai <b>pamurba lagu</b>, pemimpin alur melodi."),
            new Info("Siter", "Siter", "Kecapi petik berdawai banyak", "Petik",
                "Memberi iringan lembut dan hiasan melodi yang halus pada gamelan lembut.",
                "Siter diletakkan di lantai atau di pangkuan pemain. Dawai yang dipasang berpasangan dipetik dengan kuku ibu jari kedua tangan, sementara jari-jari lainnya meredam dawai yang baru dipetik agar nada tidak bertumpuk. Pola permainannya rapat dan mengalir, mengisi sela-sela balungan.",
                "Siter termasuk keluarga kecapi (<i>zither</i>) yang dikenal di banyak daerah Nusantara. Dalam gamelan Jawa dikenal siter dan <b>celempung</b>; celempung berukuran lebih besar dan bertumpu pada kaki penyangga. Siter juga dipakai dalam ansambel kecil bernama <i>siteran</i>."),
            new Info("Suling", "Suling", "Seruling bambu bernada lembut", "Tiup",
                "Memberi warna melodi yang lembut dan menghias lagu dalam gamelan lembut.",
                "Suling ditiup dari ujung atas yang diberi lilitan sehingga udara terarah ke celah tiup. Tinggi nada diatur dengan membuka dan menutup lubang jari serta mengatur kekuatan tiupan. Suling dimainkan bebas, tidak terikat ketukan, dengan liukan nada yang mengisi sela-sela lagu, terutama pada gendhing lembut.",
                "Seruling bambu tergolong alat musik tertua di Nusantara dan sudah tergambar pada relief Candi Borobudur. Dalam gamelan Jawa, suling dibuat terpisah untuk laras slendro (umumnya empat lubang) dan pelog (umumnya lima atau enam lubang)."),
        };

        public static InstrumentDatabase BuildAll()
        {
            Directory.CreateDirectory(DataDir);
            Directory.CreateDirectory("Assets/Resources");

            var db = AssetDatabase.LoadAssetAtPath<InstrumentDatabase>(DbPath);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<InstrumentDatabase>();
                AssetDatabase.CreateAsset(db, DbPath);
            }

            var list = new List<InstrumentData>();
            foreach (var info in All)
            {
                string path = $"{DataDir}/{info.id}.asset";
                var d = AssetDatabase.LoadAssetAtPath<InstrumentData>(path);
                if (d == null)
                {
                    d = ScriptableObject.CreateInstance<InstrumentData>();
                    AssetDatabase.CreateAsset(d, path);
                }
                d.id = info.id;
                d.displayName = info.nama;
                d.tagline = info.tagline;
                d.kelompok = info.kelompok;
                d.laras = LarasType.Keduanya;
                d.fungsi = info.fungsi;
                d.teknik = info.teknik;
                d.sejarah = info.sejarah;
                d.referenceImageName = "Kartu_" + info.id;

                var standar = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/{info.id}.fbx");
                var detail = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/{info.id}Set.fbx");
                d.modelStandar = standar != null ? standar : detail;
                d.modelDetail = detail != null ? detail : standar;
                if (standar == null && detail == null) Debug.LogWarning($"[GamelAR] Model {info.id}.fbx tidak ditemukan di Assets/Models");

                var thumb = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/UI/Thumbs/{info.id}_set.png");
                if (thumb != null) d.thumbnail = thumb;

                EditorUtility.SetDirty(d);
                list.Add(d);
            }
            db.instruments = list;
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GamelAR] {list.Count} data instrumen siap di {DataDir}");
            return db;
        }

        public static string[] AllIds()
        {
            var ids = new string[All.Length];
            for (int i = 0; i < All.Length; i++) ids[i] = All[i].id;
            return ids;
        }
    }
}
