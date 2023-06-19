# Mania Strain Calculation Primer

`StrainValueOf` governs how note strain is evaluated, which in-turn affects how star rating is calculated.

---

## Theory

Strain refers to the difficulty of the note.

There are 3 strain values present.

- **Global Strain** $GS$: depends on all previous notes.
- **Column Strain** $CS$: depends on all previous notes in the same column.
- **Strain** $S$: a function of GS and CS. $S=S(GS, CS)$

### Intuition

Take for example a simple strain calculator:

- An UPSCROLL 3K Map denoted on the last 3 columns, with 3 notes, denoted by X
- Each note increments both strains by 5
- Each timestep decays both strain by 1 till 0
- Strain is the sum of Global and the note's column-th Column Strain

| Obj # | GS | CS0 | CS1 | CS2 | S    | K0 | K1 | K2 |
|-------|----|-----|-----|-----|------|----|----|----|
| Init  | 1  | 0   | 0   | 0   | .    |    |    |    |
| 1     | 6  |     |     | 5   | 6+5  |    |    | X  |
|       | 5  |     |     | 4   | .    |    |    |    |
| 2     | 10 |     | 5   | 3   | 10+5 |    | X  |    |
|       | 9  |     | 4   | 2   | .    |    |    |    |
| 3     | 14 |     | 3   | 6   | 14+6 |    |    | X  |
|       | 13 |     | 2   | 5   | .    |    |    |    |

Notice that:

- GS increments every note regardless of column, while only the CS increments.
- S is dependent on GS and CS, never itself.
    - On the 2nd note, S=GS+CS1 ignoring CS2 because the note's column is 1
    - Same goes for the 3rd note, S=GS+CS2
- CS is a matrix, GS is a vector.

While the actual script is non-trivial, it has similar concepts from the above example:

- Our decay is exponential, so high strain values are exponentially rarer
- We consider Hold body interactions, notes in complex Hold scenarios are have boosted strains.
- Decay is zero when evaluating notes at the same time (e.g. chords)
- Strain is still the sum of Global and Column Strain

### Strains

To define our behavior robustly, we'll use math notations:

- $GS_i$: GS on the $i^{th}$ object
- $CS_{i,k}$: CS on the $i^{th}$ object on column $k$.
- $S_i$: S on the $i^{th}$ object.
- $O_i$: $i^{th}$ note/object
- $\Delta_i$: time elapsed between the $i^{th}$ and previous object
- $\Delta_{i,k}$: time elapsed between the $i^{th}$ and previous object in the same column $k$
- $\alpha$: decay constant
- $K$: Total keys
- $N$: Total notes

Where $GS$, $S$ are vectors, and $CS$ is a matrix of strains

$$
GS\in\mathbb R^N, CS\in\mathbb R^{N\times K}, S\in\mathbb R^N
$$

The 1st $GS_0$ is initialized with a value $GS_0=1$

Subsequent GS depends on

- Previous value $GS_{i-1}$
- Current and Past Notes $O_{i, i-1, ...}$
- Strain Decay $Decay$

$$GS_i=f(GS_{i-1}, O_{i, i-1, ...}, Decay(\Delta_i,\alpha_{GS}))$$

The 1st $CS_0$ is initialized with $CS_0=(0,0,...,0)\in\mathbb R^K$

Subsequent $CS_t,k$ values depend on

$$CS_i=f(CS_{i-1}, O_{i, i-1, ...}, Decay(\Delta_{i,k},\alpha_{CS}))$$

Finally, $S_i=GS_i+CS_{i,k}$ where $k$ is the note column

## Evaluating Strains

To evaluate $GS_i$:

1) We decay: $x=GS_{i-1}\times (\alpha_{GS})^{\Delta_i}$
2) Add bonus $B$ given the current and past notes: $GS_i=x+B|O_{i, i-1, ...}$

Similarly for $CS_i$:

1) We decay: $x=CS_{i-1}\times (\alpha_{CS})^{\Delta_{i,k}}$
2) Add bonus $B$ given the current and past notes: $CS_i=x+B|O_{i, i-1, ...}$

> The bonuses are explained in the [Hold Bonus Evaluation](#hold-bonuses-evaluation)

---

## Implementation

### Rules

Notes fed into `StrainValueOf` follow these rules:

- Only 1 note is fed in at a time.
- They are time sorted by the note head.
- The first note is omitted.

> - The 1st & 2nd rule implies that notes at the same time (e.g. in a chord)
    > don't iterate into `StrainValueOf` deterministically.
> - The 2nd rule implies the note time, if it's not a long note.
> - The 3rd rule is due to a requirement that all notes need to have a reference.
    See [CreateDifficultyHitObjects](../ManiaDifficultyCalculator.cs).

---

### Hold Strain Bonus Triggers

Holds trigger a strain bonus given its interaction with the current note.

Bounded by the [rules](#rules) above, Hold handling is non-trivial as
we are only allowed to see previous notes.

To better understand what are the possibilities, we illustrate all possible states.
Impossible cases are marked with `X` as [notes are sorted](#rules)

```
Legend             E.g. 
+--------------+   +-------------+
| (State Name) |   | (B2)        | Currently, 
| Prev Note    |   | [====]      | we have a note (O) & a hold started before this note 
| This Note    |   |      O      | then ended on the same time as us.
+--------------+   +-------------+

       The column titles are with respect to the previous note.
       E.g. B2: Long Note End is On the Head of the current Note 
      +-------------+-------------+-------------+-------------+--------------+
      | Before Head | On Head     | On Body     | On Tail     | After Tail   |
      | (1)         | (2)         | (3)         | (4)         | (5)          |
+-----+-------------+-------------+-------------+-------------+--------------+
| (A) | (A1)        | (A2)        |             |             |              |
|     | O           |      O      |      X      |      X      |      X       |
|     |      O      |      O      |             |             |              |
+-----+-------------+-------------+-------------+-------------+--------------+
| (B) | (B1)        | (B2)        |             |             | (B5)         |
|     | [==]        | [====]      |      X      |      X      | [==========] |
|     |      O      |      O      |             |             |      O       |
+-----+-------------+-------------+-------------+-------------+--------------+
| (C) |             |             |             |             | (C5)         |
|     |      X      |      x      |      X      |      X      |      [===]   |
|     |             |             |             |             |      O       |
+-----+-------------+-------------+-------------+-------------+--------------+
| (D) | (D1)        | (D2)        |             |             |              |
|     | O           |      O      |      X      |      X      |      X       |
|     |      [===]  |      [===]  |             |             |              |
+-----+-------------+-------------+-------------+-------------+--------------+
| (E) | (E1)        | (E2)        | (E3)        | (E4)        | (E5)         |
|     | [==]        | [====]      | [======]    | [=========] | [==========] |
|     |      [===]  |      [===]  |      [===]  |       [===] |      [===]   |
+-----+-------------+-------------+-------------+-------------+--------------+
| (F) |             |             | (F3)        | (F4)        | (F5)         |
|     |      X      |      X      |      [=]    |       [===] |      [=====] |
|     |             |             |      [===]  |       [===] |      [===]   |
+-----+-------------+-------------+-------------+-------------+--------------+
```

In our script, we trigger strain bonuses under 2 conditions

- Column 3: `endOnBodyBias`
- Column 5: `endAfterTailWeight`

> Weight and bias refers to the multiplication and addition of strain respectively

#### End On Body Bias

Given Column 3 [states](#hold-strain-bonus-triggers), the bias is $b(r)=1/\left(1+\exp(0.5(R-r))\right)$

- $r$ is measure of the Hold intersection in milliseconds
- $R$ is the Release Threshold, a constant

To visualize this,

```
            End on Body Bias
                ^
            1.0 + - - - - -  ------------
                |           /
            0.5 + - - - -  /  
                |         /|
            0.0-+----------+---------------> Intersection Length / ms
                |          R
State E3  [=============]
State F3        [=======]
This            [============================]
```

> By design, this bias only affects GS

#### End After Tail Weight

Given Column 5 [states](#hold-strain-bonus-triggers), the weight is $w=1.25$ else $w=1$

### Bonuses Evaluation

We evaluate the scenarios including [Hold Bonus Triggers](#hold-strain-bonus-triggers) to find strain bonus $B$ used in [Evaluating Strains](#evaluating-strains)

|                 | CS    | GS           |
|-----------------|-------|--------------|
| Default         | $+2$  | $+1$         |         
| If EndAfterTail | $+2w$ | $+w$         |      
| If EndOnBody    |       | $+1+b(r)$    |     
| If Both         |       | $+w(1+b(r))$ |  

---

### Maximizing Strain Summation for Deterministic Chord Strains

We know $S_i=GS_i+CS_{i,k}$. However, given our [2nd Rule](#rules)
and [Hold Strain Bonuses](#hold-strain-bonus-triggers). $S_i$ within a chord can be non-deterministic given a fixed map.

For example, if we had a 2K map that had a single note, followed by a 2-note chord.

We'll illustrate this with the [table we had earlier with the same conditions](#intuition).

| Obj # | GS | CS0 | CS1 | S    | K0 | K1 |
|-------|----|-----|-----|------|----|----|
| 1     | 6  |     | 5   | 6+5  |    | X  |
|       | 5  |     | 4   | .    |    |    |
| 2     | 10 |     | 9   | 10+9 |    | X  |
| 3     | 15 | 5   | 9   | 15+5 | X  |    |

Our resulting $S$ would be $[11, 19, 20]$

Because of our [2nd Rule](#rules), the order of note fed is non-deterministic.
Thus the following is also possible.

| Obj # | GS | CS0 | CS1 | S    | K0 | K1 |
|-------|----|-----|-----|------|----|----|
| 1     | 6  |     | 5   | 6+5  |    | X  |
|       | 5  |     | 4   | .    |    |    |
| 2     | 10 | 5   |     | 10+5 | X  |    |
| 3     | 15 | 5   | 9   | 15+9 |    | X  |

Our resulting $S$ would be $[11, 15, 24]$.

#### Solution

In order to solve this problem, when for notes within a chord, we always take the maximum CS.

| Obj # | GS | CS0         | CS1 | S    | K0 | K1 |
|-------|----|-------------|-----|------|----|----|
| 1     | 6  |             | 5   | 6+5  |    | X  |
|       | 5  |             | 4   | .    |    |    |
| 2     | 10 |             | 9   | 10+9 |    | X  |
| 3     | 15 | max(5,CS)=9 | 9   | 15+9 | X  |    |

Here, `max(5,CS)=max(5,(9,))=9`, $S=[11,19,24]$

| Obj # | GS | CS0 | CS1         | S    | K0 | K1 |
|-------|----|-----|-------------|------|----|----|
| 1     | 6  |     | 5           | 6+5  |    | X  |
|       | 5  |     | 4           | .    |    |    |
| 2     | 10 | 5   |             | 10+5 | X  |    |
| 3     | 15 | 5   | max(9,CS)=9 | 15+9 |    | X  |

`max(9,CS)=max(9,(5,))=9`. $S=[11,15,24]$.

However, it didn't matter that $S$ is different, because the
function [Process](../../../osu.Game/Rulesets/Difficulty/Skills/StrainSkill.cs), aggregates strain by `SectionLength` ms
windows before calculating star rating.

Therefore only $\max(S)$ mattered, which is already made deterministic by the above algorithm.
