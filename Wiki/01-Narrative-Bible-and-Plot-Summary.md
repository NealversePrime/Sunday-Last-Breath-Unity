# 01. Narrative Bible & Complete Plot Summary

> **[⬅️ Back to Portfolio Home](Home.md)** | **[Next: Technical Narrative & Branching Systems ➡️](02-Technical-Narrative-and-Branching-Systems.md)**

---

## 🌎 Worldbuilding, Setting & Timeline

### The Pathogen ("The Rage / Category 5 Neuro-Pathogen")
Unlike traditional slow-shambling undead tropes, the outbreak in **Sunday's Last Breath** is caused by a hyper-aggressive neuro-pathogen with an **87% urban infection rate** within the first six hours. Victims experience spiking fevers, joint burning, and violent neurological seizures before losing higher cortical function, turning into sprinting, hyper-violent hosts driven by acoustic and visual stimuli.

### Chronological Countdown: Saturday Night to Sunday Dawn
The narrative takes place over a compressed, high-tension window across **Seattle and rural King County, Washington**:
- **21:46 (Saturday, Oct 25, 2025):** King County Sheriff's Office issues an Emergency Alert System (EAS) Civil Danger Warning.
- **23:45 (Prologue Start):** The player wakes up drenched in cold sweat inside **Unit 404 of The Ascott Residency** with **15 minutes** before midnight.
- **00:00 (Midnight — Act 1 Start):** Power grids fail across the district; the Ascott Residency descends into chaos as infected breaches occur on multiple floors.
- **01:30 – 03:30 (Act 2 & Act 2.5):** The player escapes into the burning city streets, navigates the gridlocked **I-5 Southbound corridor**, refuels at a dark rural gas station on **Route 9**, and faces mid-journey crises on **Rural Route 11** and/or **Oakhaven Cabin**.
- **04:00 (Night Arrival) OR 06:00+ (Dawn Arrival — Act 3):** Depending on whether the player took the northern ridge detour to Oakhaven Cabin (`_arrivedLate`), the player reaches the **Timberline Outskirts** either in the pitch-black pouring rain or in the freezing gray dawn after the military's **06:00 Operation WILDFIRE firebombing purge** of the urban core.

---

## 🎬 Prologue: The Final Fifteen Minutes (`Unit 404`)

**Source Files:** [`Prologue.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue.twee), [`Prologue_Phone.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue_Phone.twee), [`Prologue_Radio.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue_Radio.twee), [`Prologue_Hacking.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue_Hacking.twee)

### 1. The Cold Open
The story opens with an ASCII-styled **Emergency Alert System (EAS)** terminal broadcast from the King County Sheriff's Office ordering all citizens to barricade doors and windows. The player awakens at **23:45** from a suffocating nightmare as sirens wail through the rain-streaked glass of **Apartment 404**.

### 2. Reactive Internal Monologue
Immediately upon waking, the narrative adapts to the player's **Profession** and **Traits**:
- **Paranoid Trait:** Recognizes that the acoustic cadence of the sirens outside is wrong—they aren't clearing intersections; they're fleeing them.
- **Police Officer:** Identifies a **Code-3 mass response** and knows that when sirens overlap without grid coordination, central dispatch has collapsed.
- **Doctor:** Recognizes the quarantine broadcast terminology as total biological containment failure rather than a riot.

### 3. The 15-Minute Information Triage (`23:45` to `00:00`)
Each action advances the in-game clock by **5 minutes**. Before the midnight breach forces the player out of Unit 404, they can investigate three diegetic devices:
1. **Check the Cracked Smartphone (`Prologue_Phone`):**
   - Reveals a chaotic X/Twitter social feed (`#SeattleLockdown`) showing the war between municipal cover-ups (`@CityScanner_WA` claiming a water main break) and panicked ground-level videos (`@anon_9921`).
   - **Critical Gameplay Clue:** A direct SMS from neighbor **Professor Arthur Harmon (Unit 408)** reveals his digital keypad PIN (`8445`) and begs the player to retrieve his medical research if he doesn't make it back.
2. **Tune the Analog Police Scanner (`Prologue_Radio`):**
   - Intercepts `FREQ: 154.790 MHz`. Officer `UNIT 4-CHARLIE` pleads for backup at 4th and Pike as sedatives fail on biting attackers. `DISPATCH` officially suspends standard rules of engagement: *"Lethal force is authorized. Aim for the head."*
3. **Breach the Encrypted Terminal (`Prologue_Hacking`):**
   - Requires `Tech >= 1` or `IT Specialist`. Hacks `SYS_ADMIN - SECURE_NODE_04`, uncovering a classified Department of Defense directive: **Operation WILDFIRE**.
   - **Major Plot Reveal:** Projected infection in Sector 4 is **87%**. All evacuation corridors are being sealed, and a **total thermal firebombing purge of the city is scheduled for 06:00 hours**. The player also downloads a **Tier-1 Military Clearance Passkey**.

At **00:00**, heavy, frantic pounding erupts in the 4th-floor corridor outside. The player grabs their starting gear and steps into the hallway.

---

## 🏢 Act 1: The Ascott Residency

**Source Files:** [`Hallway.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Hallway.twee), [`Unit_408.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_408.twee), [`Unit_405.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_405.twee), [`Unit_402.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_402.twee), [`Unit_401.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_401.twee), [`Utility_Closet.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Utility_Closet.twee), [`Stairwell.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Stairwell.twee), [`Floor2_Hallway.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Floor2_Hallway.twee), [`Lobby_Entrance.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Lobby_Entrance.twee)

### 1. The 4th Floor Apartments (Micro-Stories & Long-Range Payoffs)
The 4th floor functions as a non-linear exploration hub where every apartment tells a distinct human reaction to the end of the world:

| Location | Narrative Theme | Key Mechanics & Long-Range Payoffs |
| :--- | :--- | :--- |
| **Unit 408 (Prof. Harmon's Lab)** | *The Desperate Academic* | Protected by an electronic keypad. Can be opened cleanly with PIN `8445` (from `Prologue_Phone`), `PER 6` (reading finger oils on keys `4, 5, 8`), `Locksmith`, `Tech 2`, or brute force. Contains Harmon's **Pathogen Research Notes** and medical supplies. |
| **Unit 405 (Marcus's Sanctuary)** | *The Paranoid Survivalist* | Rigged with a monofilament tripwire connected to a flashbang/shotgun mechanism. Disarming it yields **Marcus's Topographical Map** (unlocking the **Act 2.5 Oakhaven Cabin** branch), **Marcus's Leather Journal**, and his custom **Hunting Knife**—both of which are essential to peacefully resolving the armed standoff with his wife **Diane** in Act 2.5. |
| **Unit 402 ("The Quiet Room")** | *Disinformation Tragedy* | Chopin's *Nocturne in E-flat major* plays on a vinyl turntable while the smell of bleach hangs in the air. The elderly **Anderson couple** took their own lives with poison after reading a fake internet rumor that the military was gassing apartment blocks. Yields a **`.38 Revolver`** (if the player bears the psychological weight of taking it from Mr. Anderson's hand). |
| **Unit 401 ("The Party That Ended")** | *Hedonism Interrupted* | Muffled pop music loops under strobe lights and spilled drinks. A blond partygoer lies crushed beneath a 300-lb solid oak bookshelf, clutching **Sedan Keys**. Lifting the shelf requires `STR 6`, `Construction Worker`, `Breacher`, a `Crowbar`, or winning a `STR`-scaled QTE. |
| **Utility Closet** | *Patient Zero on Floor 4* | Heavy magnetic steel door (`Burglar`, `Tech 2`, `Crowbar`, `STR 7`, or QTE). Inside, the janitor's logbook reveals he locked himself in after feeling a "burning fever in his joints"—explaining the blood smears inside. Yields a **Flashlight**, **Bleach**, **Duct Tape**, and **Rope**. |

### 2. The Stairwell & 2nd Floor Acoustic Consequence Chain
To reach the ground floor, the player must descend the pitch-black emergency stairwell ([`Stairwell.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Stairwell.twee)):
- Using a **Flashlight**, **Lighter**, or moving **Slowly** (+5 minutes) keeps the descent silent.
- Attempting to **Sprint in the dark** without `AGI >= 5` causes the player to trip over debris, taking damage and setting `$made_noise = true`.
- **Immediate Payoff (`Floor2_Hallway.twee`):** When the player opens the 2nd-floor fire door, an infected tenant ("The Twitching Stranger") stands in the hall. If `$made_noise == false`, its back is turned, allowing a silent stealth bypass or an ambush first-strike. If `$made_noise == true`, the creature is already sprinting straight at the door the split-second it opens!

### 3. Climax of Act 1: The Ascott Grand Lobby Triage
In [`Lobby_Entrance.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Lobby_Entrance.twee), the player emerges into the shattered glass lobby where **four simultaneous crises** demand immediate prioritization:
1. **By the Elevators — Vane:** An androgynous lifestyle content creator (**Vane**) is cornered by a frenzied infected, desperately swinging a heavy Pelican camera case. Intervening saves his life and recruits him as the player's first companion (`$vane_status = "joined"`).
2. **Behind the Front Desk — Maya & Lily:** **Maya** is shielding her terrified 7-year-old niece **Lily**. She plans to run toward the police barricade outside. The player can warn her that the police are shooting civilians or urge her to flee with them.
3. **Center Floor — The Dead Soldier's Backpack:** A military rucksack lies in the open. Stopping to loot it while people are screaming for help grants high-tier supplies (`MRE`, `9mm Ammo`, `First Aid Kit`) at the cost of **`-10 Humanity`**.
4. **The Street Barricade:** Two panicked police officers—a hardened veteran and a bleeding, bitten rookie—are losing control of the perimeter outside.

---

## 🛣️ Act 2: Urban Collapse & The Highway

**Source Files:** [`Act2_TheStreet.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_TheStreet.twee), [`Act2_ParkingLot_Combat.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_ParkingLot_Combat.twee), [`Act2_TheRoad.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_TheRoad.twee), [`Act2_Highway.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_Highway.twee), [`Act2_GasStation.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_GasStation.twee)

1. **The Street Stampede & Alleyway Moral Check:**
   - The police barricade collapses in gunfire and screaming crowds. Cutting through a dark alleyway toward the parking garage, the player is ambushed by an infected jumper. Fighting it preserves the path; fleeing in panic leaves innocent stragglers behind to be mauled (`-5 Humanity`).
2. **Parking Garage Extraction (Vane Payoff):**
   - At the player's sedan, a hulking infected mechanic attacks as the player fumbles with the keys.
   - **If Vane was rescued in the Lobby:** Vane hot-starts/takes the wheel of the sedan and broadsides the infected mechanic at 40 mph, saving the player from a brutal solo boss fight.
3. **The Emotional Anchor (`Act2_TheRoad`):**
   - As the burning Seattle skyline shrinks in the rearview mirror, the player defines **who is waiting for them in Timberline** (`$anchor`):
     - **Wife / Girlfriend, Husband / Boyfriend, or Non-Binary Partner** (with custom or preset name entry)
     - **Parents**
     - **No One ("Surviving for Myself")**
   - This choice dynamically rewrites Act 3's emotional climax at Timberline Regional High School.
4. **I-5 Southbound Gridlock & Route 9 Pit Stop:**
   - The highway is a graveyard of abandoned cars; an Emergency Broadcast announces that military containment lines have fallen. Running low on fuel, the player diverts to the **Route 9 Timberline Pit Stop** ([`Act2_GasStation.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_GasStation.twee)).
   - The fuel pump requires a 4-digit authorization PIN. Rather than guessing, the player can brave the pitch-black convenience store aisles to find a crumpled thermal receipt reading **`PUMP AUTH: 6821`**.

---

## 🌲 Act 2.5: Divergent Midpoint Routes

Depending on the player's route and whether they discovered **Marcus's Topographical Map** in Unit 405, Act 2.5 branches into two high-stakes character encounters:

### Branch A: Rural Route 11 — The Police Cruiser Wreckage (`Act2_5_MayaCrash`)
- **The Irony:** Driving down a pitch-black forest road, the player's headlights sweep across a police cruiser wrapped around a pine tree, its horn blaring endlessly. Inside the front seat, the bitten rookie cop from the Ascott barricade has fully turned, thrashing against the steel mesh partition while **Maya and Lily** scream in the jammed backseat.
- **The Moral Triad:**
  1. **Heroic Extrication (`+10 Humanity`):** Risk the blaring horn to get them out using a **Crowbar** (pry the bent frame), **Mechanic / Tech 2** (cut the battery cables to silence the horn first), **Vane** (companion distraction), or **Brute Combat**. Maya apologizes for trusting the police over the player, and **Maya & Lily join the party** (occupying **2 car seats**).
  2. **Cold Extortion (`-25 Humanity`):** Tap the gun barrel on the glass and force Maya to pass her **First Aid Kit** and **Bottled Water** through the crack before rescuing them. Maya joins the car in venomous silence (`$maya_state = "extorted"`).
  3. **Abandonment (`-40 Humanity`):** Put the car in drive and leave them to die. If **Vane** is in the passenger seat, he is horrified by the player's cruelty, permanently shattering his trust (`$vane_trust = "broken"`).

### Branch B: The Northern Ridge — Oakhaven Cabin Standoff (`Act2_5_MarcusCabin`)
- **Unlock Condition:** Requires scavenging the **Topographical Map** from Marcus's trapped apartment (`Unit 405`) in Act 1.
- **The Standoff:** Hidden high in the freezing pine forest, the player approaches Marcus's off-grid cabin (`Oakhaven`). The front door flies open and **Diane**—Marcus's wife—levels a double-barreled shotgun at the player's chest while shielding her two young sons behind her legs.
- **Humanity-Gated Dialogue & Act 1 Item Payoff:**
  - The player's initial verbal approach is gated by their current **Humanity (`$hum >= 70`, `31–69`, or `<= 30`)**.
  - When Diane demands to know how the player found the cabin, having **Marcus's Leather Journal (`+10 Humanity`)** or **Marcus's Custom Hunting Knife (`+15 Humanity`)** from Unit 405 proves that Marcus died trying to prepare a safe haven for her.
  - Diane lowers the shotgun in tears and joins the player's group with her two boys (`$diane_status = "joined"`, occupying **3 car seats**).
- **World-State Consequence (`_arrivedLate = true`):** Because the mountain detour takes hours, visiting Oakhaven Cabin advances the timeline from **Night (04:00)** to **Dawn (06:00+)** for all of Act 3.

---

## 🏫 Act 3: Timberline Outskirts & The Sanctuary

**Source Files:** [`Act3_Home.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Home.twee), [`Act3_Sheriff.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Sheriff.twee), [`Act3_School_GFloor.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_School_GFloor.twee), [`Act3_Meeting_Evelyn.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Meeting_Evelyn.twee), [`Act3_7Eleven_Combat.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_7Eleven_Combat.twee)

### 1. The Colonial House & Sheriff Substation 4 (`Officer Reyes`)
Arriving on the outskirts of Timberline, the environment reflects `_arrivedLate` (either pitch-black rain with a fresh candle burning in the window and warm coffee on the stove, or pale morning frost with cold coffee grounds and a puddle of dead wax).
- At **King County Sheriff Substation 4** / the Colonial House, the player encounters **Officer Reyes**, a wounded, pragmatic deputy holding out against the infected.
- **The 4-Seat Vehicle Capacity Dilemma (`_carCount`):**
  - The player's sedan holds a maximum of **4 seats**.
  - `Player (1)` + `Vane (1)` + `Maya & Lily (2)` + `Diane & 2 Boys (3)`.
  - If the car is already full (`_carCount >= 4`), Reyes physically cannot fit inside the vehicle; the player must coordinate a separate tactical route with him rather than extracting him directly in the backseat.

### 2. Timberline Regional High School & Matriarch Evelyn
The fortified gymnasium of **Timberline Regional High School** serves as the survivors' final bastion, commanded by **Matriarch Evelyn** and military liaison **Commander Hayes**.
- **Dynamic Anchor Payoff (`Act3_Meeting_Evelyn`):**
  - **If `$anchor == "parents"`:** Matriarch Evelyn is revealed to be **the player's own mother**, turning a formal refugee intake inspection into an emotional family reunion where Evelyn struggles to balance her duty as camp leader with relief that her child survived Seattle.
  - **If `$anchor == "partner_f" / "partner_m" / "partner_nb"`:** The player discovers their partner (`$anchor_name`) was part of the volunteer supply convoy that became pinned down at the local **7-Eleven**.
  - **If `$anchor == "none"` (Lone Wolf):** Evelyn evaluates the player purely as a hardened operative needed to keep the sanctuary alive.

### 3. Climax: The 7-Eleven Rescue & Supply Operation
To secure the school's survival (and rescue their missing anchor/scavengers), the player teams up with **Officer Reyes** and **Vane** to breach a swarm-surrounded **7-Eleven** convenience store ([`Act3_7Eleven_Combat.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_7Eleven_Combat.twee)), culminating in a multi-phase tactical extraction where every skill, weapon, and companion bond forged since Unit 404 is put to the ultimate test.

