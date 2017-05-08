Configuration
=============

Create a .ini file with a section "Wahlen" that lists the different elections, giving the name
of the section (which will also be shown in the GUI as name of the tab). Each election section
can contain a "Text=" with a short description that explains the election. This will show up
in the GUI in the election tab.
The field "Typ=" configures the type of the election. Valid values are:
- JN - this is only valid if there is only one nominee. The nominee will be shown in the UI with
  yes/no radio buttons next to the name.
- Punktesystem - the votes are weighted according to their rank (nominee on first vote gets
  higher weight than on second vote)
The field "Stimmen=" specifies the number of votes that the votee has.
The field "Title=" specifies the overall name of the election and will be shown as the title of
the GUI.
The field "ZwischentextVorX=" specifies a text that will be displayed before the selection field
X. It serves as a in-between heading.
The field "LimitKandidatY=a-b" limits the availability of nominee Y on the selections
between a and b.

[Wahlen]
Title=The big election 2016
Wahl1=First Election
Wahl2=Second Election

[First Election]
Text=A short description of the election
Typ=JN
Stimmen=1
Kandidat1=Mickey Mouse

[Second Election]
Text=Some desccription
Typ=Punktesystem
Stimmen=3
Kandidat1=Mickey Mouse
Kandidat2=Donal Duck
LimitKandidat2=2-3
Kandidat3=Dagobert Duck
Kandidat4=Goofy

