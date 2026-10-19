# 05. Selected Writing Samples (Portfolio Excerpts)

> **[⬅️ Previous: Diegetic UI & Environmental Storytelling](04-Diegetic-UI-and-Environmental-Storytelling.md)** | **[🏠 Portfolio Home](Home.md)**

---

## 📋 Overview for Narrative & Game Design Reviewers

This document contains **five curated writing samples** extracted directly from the playable build of **Sunday's Last Breath**, formatted to showcase both **literary voice** and **technical narrative implementation**:

1. **Sample 1: Diegetic Social Feed & Emergency Broadcast (`Prologue` & `Prologue_Phone`)** — Demonstrates found-media UI writing, institutional vs. citizen voice differentiation, and embedded puzzle clues.
2. **Sample 2: Environmental Tragedy & Sensory Prose (`Unit 402 — The Quiet Room`)** — Demonstrates atmospheric horror, restraint, and environmental storytelling without spoken dialogue.
3. **Sample 3: High-Tension Multi-Crisis Triage (`Act 1 — The Ascott Grand Lobby`)** — Demonstrates pacing, urgent action prose, and simultaneous choice architecture.
4. **Sample 4: Morality-Gated Standoff & Long-Range Item Payoff (`Act 2.5 — Oakhaven Cabin`)** — Demonstrates reactive dialogue across three `Humanity ($hum)` brackets and cross-act item payoffs.
5. **Sample 5: Chronological & Companion State Reactivity (`Act 3 — The Colonial House`)** — Demonstrates combinatorial companion banter (`Vane`, `Maya`, `Diane`) and time-of-day (`_arrivedLate`) world state shifts.

---

## 📱 Sample 1: Diegetic Social Feed & Emergency Broadcast
**Source:** [`Prologue.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue.twee) & [`Prologue_Phone.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Prologue_Phone.twee)  
**Design Context:** The opening scene contrasts an official Emergency Alert System (EAS) broadcast with the player's cracked smartphone feed at 23:45, embedding the keypad code (`8445`) for Unit 408 inside a neighbor's frantic SMS.

### Excerpt:
```text
========================================================================
[ EMERGENCY ALERT SYSTEM — KING COUNTY SHERIFF'S OFFICE ]
ISSUED: OCT 25, 2025 — 21:46 PST
CIVIL DANGER WARNING — MANDATORY SHELTER-IN-PLACE

ALL RESIDENTS OF KING COUNTY AND GREATER SEATTLE METRO AREA:
A CRITICAL PUBLIC SAFETY INCIDENT IS CURRENTLY IN PROGRESS. 
DO NOT ATTEMPT TO USE I-5 OR I-90 CORRIDORS.
LOCK AND BARRICADE ALL DOORS AND WINDOWS IMMEDIATELY.
DO NOT APPROACH INDIVIDUALS EXHIBITING ERRATIC OR VIOLENT BEHAVIOR.
AWAIT FURTHER INSTRUCTIONS FROM MILITARY AND CIVIL AUTHORITIES.
========================================================================
```

You wake up choking on a mouthful of dry air, sheets twisted around your legs like wet rope. 

Outside the rain-streaked glass of Unit 404, the city is screaming. Not a single siren, but a dozen of them, overlapping in a ragged, Doppler-shifted chorus that rises and falls through the concrete canyons of downtown Seattle.

> **`[IF TRAIT == "PARANOID"]`**  
> *You don't need to look out the window to know the pattern is wrong. Sirens respond to a scene and stop. These aren't stopping. They're circling, overlapping, fleeing intersections they were supposed to hold.*
>
> **`[IF JOB == "Police Officer"]`**  
> *Your training kicks in before your pulse even settles. That's a Code-3 saturation across multiple precincts. When sirens overlap like that without grid discipline, it means central dispatch has lost the board.*

You reach for your cracked smartphone on the nightstand. The battery icon blinks at `14%`. Signal: `1 BAR (ROAMING)`.

#### `[DIEGETIC UI: X-FEED // #SeattleLockdown]`
* **`@CityScanner_WA` *(Verified Municipal Account — 22m ago)*:**  
  `[TRAFFIC & UTILITY ADVISORY]` Due to a major water main rupture and transformer fire near 4th & Pine, downtown transit corridors are temporarily suspended. Please remain indoors and keep 911 lines clear for emergency personnel.
* **`@NewsWire_Alerts` *(14m ago)*:**  
  `BREAKING:` Harborview Medical Center has been placed under total armed lockdown following reports of mass casualties and violent triage breaches inside the Emergency Department. National Guard units have been spotted staging at Lumen Field.
* **`@anon_9921` *(6m ago)*:**  
  *WTF IS HAPPENING ON PIKE?! The cops just opened fire on a crowd and they didn't even slow down!! [Attached Video: 0:08 — Shaky vertical footage through rain-streaked glass. Flashing blue strobes illuminate a line of riot police falling backward as three unarmored figures sprint directly through tear gas canisters, tackling an officer to the wet asphalt before the camera drops.]*

#### `[DIEGETIC UI: DIRECT MESSAGES (1 UNREAD)]`
* **`FROM: Prof. Arthur Harmon (Unit 408) — [23:42]`**  
  *"If you're still in the building, DO NOT go down to the street. The police at the corner aren't evacuating anyone—they're containing us. Something went wrong at the lab and it's already in the water or the air, I don't know which. If I don't make it back to 408 by midnight, my door code is **8445**. Take the research binder off my desk and get out of the city before dawn. God help us."*

---

## 🕯️ Sample 2: Environmental Tragedy & Sensory Prose
**Source:** [`Unit_402.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Unit_402.twee) (*"The Quiet Room"*)  
**Design Context:** No combat occurs in Unit 402. Instead, the scene uses sound design (Chopin's *Nocturne*) and environmental contrast to show the human cost of the internet disinformation seen in Sample 1.

### Excerpt:
The door to **Unit 402** isn't even latched. It yields to the weight of your fingertips, swinging inward without a sound.

The first thing that hits you isn't the smell of blood. It's the sharp, clinical sting of household bleach, layered beneath the warm, dusty crackle of a vinyl record spinning in the dim living room.

Chopin's *Nocturne in E-flat major* drifts softly through the apartment—gentle, unhurried, and devastatingly out of place.

On the floral-patterned sofa in the center of the room sit **Mr. and Mrs. Anderson**. They are dressed in their Sunday best—he in a pressed tweed jacket, she in a pale blue cardigan with a pearl brooch pinned at the collar. Mrs. Anderson's head rests peacefully against her husband's shoulder. Their fingers are intertwined on his knee.

Neither of them is breathing.

On the glass coffee table in front of them sit two empty crystal tumblers, a half-empty gallon jug of industrial bleach, and a printed screenshot from an internet conspiracy forum, weighted down by a brass coaster. The headline on the printout reads:

> **`LEAKED PENTAGON MEMO: MILITARY TO PUMP NERVE GAS INTO SEATTLE HIGH-RISES AT MIDNIGHT TO CONTAIN RIOTS — NO SURVIVORS.`**

Beside the printout rests a heavy, blued-steel **`.38 Revolver`**, fully loaded, its cylinder never turned. Mr. Anderson had laid it out on a folded handkerchief, looked at his wife of fifty years, and decided he couldn't bear the sound of a gunshot in their home.

They died trying to save each other from a lie they read on a screen.

---

## 🚨 Sample 3: High-Tension Multi-Crisis Triage
**Source:** [`Lobby_Entrance.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Lobby_Entrance.twee)  
**Design Context:** The climax of Act 1 drops the player into a shattered ground-floor lobby where four distinct crises unfold simultaneously. The prose establishes immediate spatial awareness and moral urgency.

### Excerpt:
You burst through the heavy fire doors into **The Ascott Grand Lobby**, and the sheer acoustic violence of the night hits you like a physical blow.

The floor-to-ceiling glass facade facing the street has been shattered inward. Freezing rain and flashing red-and-blue police strobes slice through the lobby in jagged ribbons. Everything is happening at once:

* **To your left, by the brass elevators (`CRISIS 1 — VANE`):**  
  A slender, androgynous twenty-something in a torn designer jacket is pinned against the brushed-steel doors. A former resident—jaw dislocated, eyes blown wide with blood-red vessels—is snapping at their throat. The survivor is swinging a heavy black Pelican camera case with both hands, using the reinforced plastic like a desperate battering ram. *"Get off me! Someone—please!"*
* **To your right, behind the marble reception desk (`CRISIS 2 — MAYA & LILY`):**  
  A woman in her thirties (**Maya**) is crouched low, pressing a terrified seven-year-old girl (**Lily**) against the cabinetry. Maya's eyes are locked on the street entrance, preparing to sprint across the broken glass toward the flashing police cruisers outside.
* **In the center of the terrazzo floor (`CRISIS 3 — THE DEAD MAN'S PACK`):**  
  Next to an overturned velvet armchair lies a dead National Guardsman in torn woodland camo. Strapped to his webbing is a heavy **Military Field Pack**, bulging with sealed rations and ammunition magazines. Nobody is guarding it.
* **Outside the shattered front doors (`CRISIS 4 — THE POLICE BARRICADE`):**  
  Two Seattle PD officers are braced behind the open doors of a patrol cruiser. The older officer has his service Glock leveled at the crowd; the younger officer is pale, sweating profusely, and clutching a blood-soaked rag against a deep bite wound on his forearm.

You have seconds before the street line collapses completely. **Who—or what—do you prioritize?**

---

## 🌧️ Sample 4: Morality-Gated Standoff & Long-Range Item Payoff
**Source:** [`Act2_5_MarcusCabin.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act2_5_MarcusCabin.twee)  
**Design Context:** Demonstrates how the player's **Humanity (`$hum`)** alters spoken dialogue in an armed standoff, and how scavenging **Marcus's Journal** or **Hunting Knife** in Act 1 (Unit 405) pays off two acts later.

### Excerpt:
The porch light snaps on, blinding you through the sheets of freezing mountain rain. 

The heavy timber door of **Oakhaven Cabin** bangs open. A woman in a thick flannel jacket—**Diane**—steps onto the slick cedar boards, leveling a double-barreled 12-gauge shotgun squarely at your chest. Behind her legs, two young boys peer out from the dark doorway, wide-eyed and shivering.

*"Stop right there!"* Diane's voice cracks over the thunder, her knuckles white on the stock, the twin barrels trembling slightly. *"Take one more step toward this porch and I swear to God I'll paint the mud with you! Who are you?! Where is Marcus?!"*

#### `[DYNAMIC SPOKEN DIALOGUE — GATED BY HUMANITY ($hum)]`

* **`[OPTION A — REQUIRES HIGH HUMANITY ($hum >= 70)]`**  
  > **You:** *"Easy! Keep the gun on me if it makes you feel safe, Diane, just don't pull that trigger in front of your boys. My hands are up. I came from the Ascott apartments—Marcus sent me."*  
  > *(You slowly raise both palms into the amber porch light, deliberately stepping half a pace back into the mud so she can see you aren't closing the distance.)*

* **`[OPTION B — REQUIRES MID HUMANITY ($hum 31–69)]`**  
  > **You:** *"Whoa. Hold your fire. Look at me—I'm soaking wet, I'm not biting, and if I wanted to rob you I wouldn't have walked up your front steps in the light. I found your cabin on a map in Unit 405."*

* **`[OPTION C — REQUIRES LOW HUMANITY ($hum <= 30)]`**  
  > **You:** *"Put the twelve-gauge down before you break your own collarbone with it, lady. Look at your hands—you're shaking so hard you'll miss my chest and blow a hole in your porch roof. Listen to what I have to say about your husband."*

---

#### `[PHASE 2 — THE ACT 1 ITEM PAYOFF]`
Diane doesn't lower the barrels, but her breath hitches at the mention of Unit 405. *"You were in his apartment? Why isn't he with you? He called me at nine o'clock, he said he was packing the truck—"*

* **`[IF PLAYER HAS "Hunting Knife" FROM UNIT 405 — +15 HUMANITY]`**  
  > Slowly, using only two fingers, you reach into your coat and draw **Marcus's custom hunting knife** by the sheath, setting it gently on the wet bottom step of the porch. The porch light catches the brass bolster and the initials carved into the walnut grip.  
  >  
  > **You:** *"He rigged Unit 405 to keep scavengers out while he packed for you and the boys. He didn't make it out of the city, Diane. Ask me how I got this knife—he left it beside the map to Oakhaven so someone would make sure you got out before dawn."*  
  >  
  > Diane stares down at the walnut handle. A sound escapes her throat—half sob, half exhale—and the twin barrels of the shotgun slowly sink toward the floorboards.

* **`[IF PLAYER READ "Marcus's Journal" IN UNIT 405 — +10 HUMANITY]`**  
  > **You:** *"I read his leather journal on the desk in 405. His last entry was about the two boys—how he promised to teach the oldest how to fish the creek behind this ridge comes spring. He tried to get back to you, Diane. The bridges closed before he could leave."*

---

## 🌅 Sample 5: Chronological & Companion State Reactivity
**Source:** [`Act3_Home.twee`](file:///D:/Documents/Twine/Twine%20Game/Sunday-Last-Breath/src/passages/Act3_Home.twee)  
**Design Context:** Demonstrates how the same passage dynamically shifts its atmospheric prose, companion banter (`Vane`, `Maya`, `Diane`), and weapon awareness (`_isGun`, `_isBlade`, `_isBlunt`) based on whether the player arrived at **Night (`_arrivedLate == false`)** or **Dawn (`_arrivedLate == true`)**.

### Excerpt:

#### 1. Environmental Description (`Night` vs. `Dawn` State)
> **`[IF _arrivedLate == false (04:00 AM — NIGHT ARRIVAL)]`**  
> Every window intact, every shutter still on its hinges. In a neighborhood where every other house has at least one broken pane or kicked-in door, this is conspicuous. Someone maintained this place after the world stopped maintaining anything.  
> The ground-floor window to the left of the porch has been covered from the inside with a dark blanket—thick enough to block most light. But a faint, unsteady amber glow pulses behind it. A candle. Someone is in there, holding the dark at bay.

> **`[IF _arrivedLate == true (06:15 AM — DAWN ARRIVAL AFTER OAKHAVEN CABIN)]`**  
> Every window intact, every shutter still on its hinges. In the pale gray of early morning, against a street of broken glass and kicked-in doors, the house looks almost normal. Like the world stopped just before it got here.  
> The ground-floor window to the left of the porch has been covered from the inside with a dark blanket. In daylight, the covering looks deliberate—measured, tucked at the corners. Not panicked. Along the bottom edge: a faint amber warmth. A candle burning in the middle of the morning. Either no one told them it was daylight, or they never went to sleep.

#### 2. Combinatorial Backseat Banter (`Vane + Maya` + Dynamic Weapon Check)
Vane leans forward between the front seats, chewing his lower lip nervously as he studies the covered window.

* **Vane *(if Night)*:** *"Candle in the window. Covered from the inside. They're trying to keep a low profile in the dark. Keep your eyes on the tree line, please."*
* **Vane *(if Dawn)*:** *"A candle burning in broad daylight? I-it looks like a signal... or they've been awake all night and finally passed out. We should watch the side door."*

In the backseat, Maya has seven-year-old Lily tucked securely under her arm. Her voice is careful, edged with a mother's exhaustion:

* **Maya *(if Night + Player Equipped a Firearm `_isGun`)*:**  
  *"It's freezing out here. Lily is shivering violently. If there's a heat source in there, I don't care who owns it. Just... go slow. Keep your finger off the trigger unless they force your hand. Talk first."*
* **Maya *(if Night + Player Equipped a Blade `_isBlade`)*:**  
  *"It's freezing out here. Lily is shivering violently. If there's a heat source in there, I don't care who owns it. Just... go slow. Keep that blade down unless they force your hand. Try talking first."*
* **Maya *(if Dawn)*:**  
  *"The mist is lifting. We're completely exposed out here on the curb. If we're going inside, we need to do it now before whoever is in there wakes up."*
* **You:**  
  *"Stay in the car with Lily. Doors locked. Vane, you're on the porch with me. We assess the situation before we confront anyone."*

