using System.IO;
using UnityEditor;
using UnityEngine;
using SundayLastBreath.Data;

namespace SundayLastBreath.Editor
{
    public static class CreateModularPassagesMenu
    {
        [MenuItem("Sunday's Last Breath/Generate Sample Modular Passages", priority = 200)]
        public static void GenerateSamplePassages()
        {
            string folderPath = "Assets/Resources/StoryPassages/Act1";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // Load Sprites
            Sprite hallSprite = Resources.Load<Sprite>("Act1/hall_406_407");
            Sprite room408Sprite = Resources.Load<Sprite>("Act1/unit408");

            // Load Audio
            AudioClip hallwayBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/hallway_drone.mp3");
            AudioClip tensionBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/BGM/tension_drone.mp3");

            // 1. Create or Load Assets
            string pathNorth = $"{folderPath}/Passage_Hallway_North.asset";
            string path408Door = $"{folderPath}/Passage_Unit408_Door.asset";
            string path408Inside = $"{folderPath}/Passage_Unit408_Inside.asset";
            string path406Door = $"{folderPath}/Passage_Unit406_Door.asset";

            var passNorth = AssetDatabase.LoadAssetAtPath<StoryPassageData>(pathNorth);
            if (passNorth == null)
            {
                passNorth = ScriptableObject.CreateInstance<StoryPassageData>();
                AssetDatabase.CreateAsset(passNorth, pathNorth);
            }

            var pass408Door = AssetDatabase.LoadAssetAtPath<StoryPassageData>(path408Door);
            if (pass408Door == null)
            {
                pass408Door = ScriptableObject.CreateInstance<StoryPassageData>();
                AssetDatabase.CreateAsset(pass408Door, path408Door);
            }

            var pass408Inside = AssetDatabase.LoadAssetAtPath<StoryPassageData>(path408Inside);
            if (pass408Inside == null)
            {
                pass408Inside = ScriptableObject.CreateInstance<StoryPassageData>();
                AssetDatabase.CreateAsset(pass408Inside, path408Inside);
            }

            var pass406Door = AssetDatabase.LoadAssetAtPath<StoryPassageData>(path406Door);
            if (pass406Door == null)
            {
                pass406Door = ScriptableObject.CreateInstance<StoryPassageData>();
                AssetDatabase.CreateAsset(pass406Door, path406Door);
            }

            // -----------------------------------------------------------------------------------------
            // Passage 1: Hallway North
            // -----------------------------------------------------------------------------------------
            passNorth.passageId = "hallway_1";
            passNorth.passageTitle = "CORRIDOR N // UPPER HALLWAY (SEC 1)";
            passNorth.associatedRoomId = "hallway_1";
            passNorth.backdropSprite = hallSprite;
            passNorth.musicAction = PassageMusicAction.PlayNew;
            passNorth.backgroundMusicOrAmbience = hallwayBgm;
            passNorth.musicVolume = 0.5f;
            passNorth.storyText = "Pintu apartemenmu terkunci rapat di belakangmu dengan bunyi <i>*klik*</i> berat. Kamu berada di luar sekarang.\n\n<color=#93C5FD><b>LORONG UTARA (LANTAI 4).</b></color> Lampu darurat berkedip redup, memancarkan bayangan panjang di atas karpet kotor. Udara berbau tembaga dan debu dingin.\n\nDi sebelah kirimu terdapat pintu <b>Unit 408</b> dengan keypad elektronik menyala biru. Di seberangnya adalah pintu <b>Unit 406</b> yang tertutup rapat.";
            passNorth.choices.Clear();
            passNorth.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Periksa Pintu Unit 408 (Keypad Elektronik)",
                choiceSubtitle = "Kamar Profesor Harmon. Ada keypad angka di samping pintu.",
                targetPassage = pass408Door,
                targetRoomId = "unit_408",
                timeMinutesCost = 1
            });
            passNorth.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Periksa Pintu Unit 406",
                choiceSubtitle = "Pintu tetangga di seberang. Tidak ada suara dari dalam.",
                targetPassage = pass406Door,
                targetRoomId = "unit_406",
                timeMinutesCost = 1
            });
            EditorUtility.SetDirty(passNorth);

            // -----------------------------------------------------------------------------------------
            // Passage 2: Unit 408 Door
            // -----------------------------------------------------------------------------------------
            pass408Door.passageId = "unit_408_door";
            pass408Door.passageTitle = "UNIT 408 // ELECTRONIC KEYPAD";
            pass408Door.associatedRoomId = "unit_408";
            pass408Door.backdropSprite = room408Sprite;
            pass408Door.musicAction = PassageMusicAction.KeepPlaying;
            pass408Door.storyText = "Profesor Harmon mengajar sosiologi dan sejarah keruntuhan peradaban di universitas.\n\nPintunya terkunci dengan gembok magnetik berat. Keypad kecil di samping pintu menyala biru dingin menunggu 4 digit angka.\n\nTertempel memo di atasnya: <i>\"Jika aku tidak menjawab, aku sudah tewas atau kabur. Bagaimanapun juga, kamu tidak akan bisa masuk.\"</i>";
            pass408Door.choices.Clear();
            pass408Door.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Masukkan kode 8445 dari HP",
                choiceSubtitle = "Kamu ingat pesan SMS yang kamu terima di ponsel tadi.",
                targetPassage = pass408Inside,
                targetRoomId = "unit_408",
                timeMinutesCost = 1
            });
            pass408Door.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Kembali ke lorong utama",
                choiceSubtitle = "Mundur beberapa langkah ke koridor.",
                targetPassage = passNorth,
                targetRoomId = "hallway_1",
                timeMinutesCost = 1,
                isBackButton = true
            });
            EditorUtility.SetDirty(pass408Door);

            // -----------------------------------------------------------------------------------------
            // Passage 3: Unit 408 Inside (With Loose Items & Scavenge to Floor!)
            // -----------------------------------------------------------------------------------------
            pass408Inside.passageId = "unit_408_inside";
            pass408Inside.passageTitle = "UNIT 408 // PROFESSOR HARMON'S APARTMENT";
            pass408Inside.associatedRoomId = "unit_408";
            pass408Inside.backdropSprite = room408Sprite;
            pass408Inside.musicAction = PassageMusicAction.PlayNew;
            pass408Inside.backgroundMusicOrAmbience = tensionBgm;
            pass408Inside.musicVolume = 0.4f;
            pass408Inside.storyText = "<color=#10B981><b>*BEEP-CLICK-CHUNK!*</b></color>\n\nKunci magnetik terlepas dengan bunyi dentuman berat. Pintu apartemen 408 mengayun terbuka.\n\nDi dalam, rak-rak buku menjulang tinggi memenuhi dinding. Di atas meja kerja berserakan berkas dan perlengkapan medis. Laci meja tampak terkunci rapat.";
            
            // Loose items on table/floor in this room:
            pass408Inside.initialRoomItems.Clear();
            pass408Inside.initialRoomItems.Add("bandage");
            pass408Inside.initialRoomItems.Add("matches");

            pass408Inside.choices.Clear();
            pass408Inside.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Bongkar Laci Meja Kerja (Scavenge)",
                choiceSubtitle = "Bongkar laci kayu dengan paksa [+10 Mins, peluru jatuh ke lantai].",
                givesItem = "ammo_9mm",
                givesItemQty = 12,
                dropDirectlyToFloor = true, // Drops directly to [ ROOM ITEMS ]!
                timeMinutesCost = 10,
                targetPassage = pass408Inside
            });
            pass408Inside.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Keluar kembali ke lorong",
                choiceSubtitle = "Tutup pintu dan kembali ke koridor utama.",
                targetPassage = passNorth,
                targetRoomId = "hallway_1",
                timeMinutesCost = 1,
                isBackButton = true
            });
            EditorUtility.SetDirty(pass408Inside);

            // -----------------------------------------------------------------------------------------
            // Passage 4: Unit 406 Door
            // -----------------------------------------------------------------------------------------
            pass406Door.passageId = "unit_406_door";
            pass406Door.passageTitle = "UNIT 406 // UNRESPONSIVE APARTMENT";
            pass406Door.associatedRoomId = "unit_406";
            pass406Door.backdropSprite = hallSprite;
            pass406Door.musicAction = PassageMusicAction.KeepPlaying;
            pass406Door.storyText = "Pintu kayu nomor 406 tampak terkunci dari dalam. Pegangan pintunya dingin dan kaku.\n\nKamu menempelkan telinga ke permukaan kayu yang tebal. Sunyi senyap—tidak ada suara nafas, langkah kaki, atau desah radio. Penghuninya kemungkinan besar sudah mengungsi saat peringatan darurat pertama berbunyi.";
            pass406Door.choices.Clear();
            pass406Door.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Ketuk pintu tiga kali",
                choiceSubtitle = "Mengetuk pelan untuk memastikan ada orang di dalam.",
                timeMinutesCost = 2,
                targetPassage = pass406Door
            });
            pass406Door.choices.Add(new PassageChoiceData
            {
                choiceLabel = "Kembali ke lorong utama",
                choiceSubtitle = "Mundur kembali ke koridor.",
                targetPassage = passNorth,
                targetRoomId = "hallway_1",
                timeMinutesCost = 1,
                isBackButton = true
            });
            EditorUtility.SetDirty(pass406Door);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>[Passage Generator] Successfully generated interconnected passages (North, 408 Door, 408 Inside, 406 Door) with isolated room items!</color>");
        }
    }
}
