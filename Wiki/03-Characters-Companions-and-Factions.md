# 03. Characters, Companions & Factions

> **[⬅️ Previous: Technical Narrative & Branching Systems](02-Technical-Narrative-and-Branching-Systems.md)** | **[🏠 Portfolio Home](Home.md)** | **[Next: Diegetic UI & Environmental Storytelling ➡️](04-Diegetic-UI-and-Environmental-Storytelling.md)**

---

## 👥 Character Design Philosophy

Every character in **Sunday's Last Breath**—whether an active party companion, a situational ally, or an environmental "ghost" encountered only through their apartment and logs—represents a distinct socioeconomic and psychological facet of modern Seattle colliding with sudden extinction.

---

## 1. The Protagonist ("The Survivor" — Unit 404)

- **Role:** Player-Defined Protagonist
- **Starting Location:** Unit 404, The Ascott Residency
- **Professions & Narrative Reactivity:**
  - **Police Officer:** Recognizes tactical radio codes (`Code-3`), breakdown of command hierarchies, and firearm ballistics.
  - **Doctor:** Unlocks specialized medical perception checks (`[MEDIC]`), spots hidden broad-spectrum antibiotics in Act 3, and diagnoses pathogen symptoms early.
  - **Burglar:** Masters non-destructive physical entry, bypassing industrial override cylinders (`Utility_Closet`) and apartment deadbolts (`Unit_408`) at half stamina cost.
  - **Construction Worker:** Excels at structural leverage and heavy lifting (`Unit_401` 300-lb oak shelf).
  - **Mechanic:** Understands vehicle electrical blocks (`Act2_5_MayaCrash` battery cable cut) and engine hotwiring.
  - **IT Specialist:** Breaches encrypted military nodes (`Prologue_Hacking`) and digital fuel relays (`Act2_GasStation`).
- **The Emotional Anchor (`$anchor`):**
  - Defined during the quiet highway drive in [`Act2_TheRoad.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_TheRoad.twee):
    - **Romantic Partner (`partner_f`, `partner_m`, `partner_nb`):** Custom-named spouse/partner waiting in Timberline.
    - **Parents (`parents`):** Directly links the player's bloodline to **Matriarch Evelyn** in Act 3.
    - **Lone Wolf (`none`):** *"No one. I'm surviving for me."*

---

## 2. Core Companions & Survivors

### 📸 Vane — *The Disillusioned Digital Creator*
- **First Appearance:** [`Lobby_Entrance.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Lobby_Entrance.twee) (Cornered by the elevators)
- **Visual & Voice Profile:** Androgynous, twenty-something lifestyle/tech content creator wearing a ruined designer jacket. Speaks with anxious, rapid-fire self-awareness that masks a surprisingly sharp observational mind.
- **Signature Line (`Act2_TheRoad`):**
  > *"My entire life was sponsorships, unboxing videos, aesthetic lighting... None of that prepared me for bludgeoning someone's skull in with a Pelican camera case. My entire world was on a screen, and the internet is dead."*
- **Systemic State Variables:**
  - `$vane_status`: `"joined"` | `"dead"` | `"left"`
  - `$vane_trust`: `"intact"` | `"broken"`
- **Mechanical & Narrative Payoffs:**
  - **Act 2 Parking Lot Rescue:** If saved in the lobby, Vane commandos the getaway sedan and broadsides the infected mechanic at 40 mph, saving the player's life.
  - **Act 2.5 Cruiser Distraction:** Draws the infected police officer's frenzy at the driver's window so the player can safely shatter the rear glass to extract Maya and Lily.
  - **Moral Conscience Compass:** If the player chooses to abandon Maya and Lily to die in [`Act2_5_MayaCrash.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_5_MayaCrash.twee), Vane shrinks against the passenger door in horror, setting `$vane_trust = "broken"`—realizing the player will discard him too the moment he becomes a liability.
  - **Vehicle Space:** Occupies **1 Seat**.

---

### 👩‍👧 Maya & Lily — *The Disillusioned Citizen & Her Niece*
- **First Appearance:** [`Lobby_Entrance.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Lobby_Entrance.twee) (Reception desk) -> Re-encountered in [`Act2_5_MayaCrash.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_5_MayaCrash.twee)
- **Visual & Voice Profile:** Maya is a fiercely protective aunt shielding her 7-year-old niece, **Lily**. In Act 1, Maya still believes in institutional authority, running toward the police barricade for safety.
- **Arc & Tragic Irony:**
  - The young rookie cop who evacuated Maya and Lily from the Ascott lobby was already bitten. On Rural Route 11, he turns mid-drive, wrapping the cruiser around a pine tree and trapping Maya and Lily in the backseat behind the steel mesh partition.
- **Systemic State Variables:**
  - `$maya_status`: `"joined"` | `"abandoned"`
  - `$maya_state`: `"rescued"` (Gratitude & trust) | `"extorted"` (Hostile compliance after the player demands her First Aid Kit and Water through the broken window before helping).
- **Reactive Dialogue (`Act3_Home`):**
  - Dynamically comments on the player's equipped weapon category (`_isGun`: *"Keep your finger off the trigger unless they force your hand"*; `_isBlade`: *"Keep that blade down..."*; `_isBlunt`: *"Don't start swinging that thing..."*).
- **Vehicle Space:** Occupies **2 Seats** (`Maya + Lily`).

---

### 🏡 Diane & Marcus — *The Survivalist Widow & The Unseen Architect*
- **First Appearance:**
  - **Marcus (Environmental):** [`Unit_405.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_405.twee)
  - **Diane & Two Sons (Live Standoff):** [`Act2_5_MarcusCabin.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_5_MarcusCabin.twee)
- **Narrative Design ("Two-Half Character Arc"):**
  - The player never meets **Marcus** alive, yet they learn everything about him in **Unit 405**: his paranoia, his meticulously rigged shotgun tripwire, his leather journal expressing terror that he won't reach his family in time, and his custom-engraved **Hunting Knife**.
  - Two acts later, when the player reaches **Oakhaven Cabin**, they meet the other half of the story: **Diane**, holding a double-barreled shotgun on the porch in the pouring rain to protect their two boys.
- **Resolution & Payoff:**
  - Presenting **Marcus's Journal (`+10 Humanity`)** or **Hunting Knife (`+15 Humanity`)** bridges the gap between Act 1's environmental scavenging and Act 2.5's emotional climax, proving to Diane that Marcus died loving them and that the player can be trusted.
- **Vehicle Space:** Occupies **3 Seats** (`Diane + 2 Boys`).

---

### 🚓 Officer Reyes — *The Last Holdout of Substation 4*
- **First Appearance:** [`Act3_Sheriff.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Sheriff.twee) & [`Act3_Home.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Home.twee)
- **Visual & Voice Profile:** A battered, disciplined King County Sheriff's Deputy holding the line in Timberline after central command collapsed. Pragmatic, terse, and observant.
- **Systemic Role:**
  - Acts as the tactical bridge into **Timberline Regional High School** and co-leads the **7-Eleven Supply & Rescue Operation** (`$companion_reyes`).
  - Checks `$stranger_trust` and `_carFull` (whether the player has room in the 4-seat sedan or must coordinate a separate approach).

---

### 🏫 Matriarch Evelyn & Commander Hayes — *The Timberline Sanctuary Leadership*
- **First Appearance:** [`Act3_School_GFloor.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_School_GFloor.twee) & [`Act3_Meeting_Evelyn.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Meeting_Evelyn.twee)
- **Dynamic Identity Twist:**
  - **Matriarch Evelyn** governs the civilian shelter inside Timberline Regional High School alongside **Commander Hayes** (who oversees perimeter defense).
  - If the player selected **`My Parents` (`$anchor == "parents"`)** in Act 2, Matriarch Evelyn is dynamically written as **the player's mother**—transforming the scene from a cold refugee screening into an intense, emotional family reunion under the shadow of martial law.

---

## 👻 3. Environmental "Ghosts" (Act 1 Micro-Cast)

| Character | Apartment | Role in Worldbuilding |
| :--- | :--- | :--- |
| **Prof. Arthur Harmon** | `Unit 408` | Virologist/Academic who texted the player his keypad code (`8445`) at 23:42. His apartment provides the scientific context for the pathogen's rapid neural incubation. |
| **Mr. & Mrs. Anderson** | `Unit 402` | Elderly couple who believed a viral internet hoax claiming the military was pumping nerve gas into Seattle apartments. They put on Chopin's *Nocturne*, drank bleach together on the sofa, and left behind an unfired `.38 Revolver`. |
| **The Building Janitor** | `Utility Closet` | Wrote in his maintenance log about tenants in 402 complaining of chemical smells, then noted his own spiking fever and burning joints before locking himself inside the steel utility room—turning into a monster behind a magnetic lock. |
| **The Crushed Partygoer** | `Unit 401` | A young man in a bloodstained pastel shirt pinned beneath a 300-lb oak bookshelf while upbeat pop music loops endlessly on a Bluetooth speaker. Holds the **Sedan Keys** required for the cleanest escape in Act 2. |

