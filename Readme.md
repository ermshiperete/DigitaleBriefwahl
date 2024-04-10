# Digitale Briefwahl

This project implements a digital postal vote system based on public keys. This is suitable
for use in a setting where there is an environment of trust between the members but who
still want to ensure that the ballot is secret. The application encrypts the ballot with the
public key of the election commission before it sends an email to the administration. The
administration verifies the eligibility of the voter and saves the encrypted ballot with
a random name to the drive. The election commission can then decrypt and count the ballots.

The ballot can be encrypted manually, but that is too complicated for most users.
This tool lowers the barrier by automating the necessary steps, requiring minimal knowledge of
the user.

However, for computer-savy people who distrust the tool it is still possible to save the ballot
and the public key of the election commission and do the encryption for themselves.

## Documentation

The detailed documentation can be found in the [wiki](https://github.com/ermshiperete/DigitaleBriefwahl/wiki).

## Configuration

Create a `wahl.ini` file with a section `Wahlen` that lists the different elections, giving the name
of the section (which will also be shown in the GUI as name of the tab). The field `Email=` contains
the email address that will receive an email with the votes. The field `PublicKey=` contains the
filename of the public key that will be used to encrypt the ballot.

Each election section can contain a `Text=` with a short description that explains the election.
This will show up in the GUI in the election tab.

The field `Typ=` configures the type of the election. Valid values are:

- `YesNo` - The nominees will be shown in the UI with yes/no/abstention radio buttons next to the name.
- `Weighted` - the votes are weighted according to their rank (nominee on first vote gets
  higher weight than on second vote etc.)

The field `Stimmen=` specifies the number of votes that the voter has.

The field `Title=` specifies the overall name of the election and will be shown as the title of
the GUI.

The field `ZwischentextVorX=` specifies a text that will be displayed before the selection field
`X`. It serves as a in-between heading.

The field `LimitKandidatY=a-b` limits the availability of nominee `Y` on the selections
between `a` and `b`.

If the optional field `FehlendOk` is set to `true` then missing/empty votes in a
weighted election are acceptable, otherwise they are treated as invalid
(default).

`wahl.ini`:

```ini
[Wahlen]
Title=The big election 2017
Wahl1=First Election
Wahl2=Second Election
Email=election@example.com
PublicKey = 12345678.asc

[First Election]
Text=A short description of the election
Typ=YesNo
Stimmen=1
Kandidat1=Mickey Mouse

[Second Election]
Text=Some description
Typ=Weighted
FehlendOk=false
Stimmen=3
ZwischentextVor1=The famous mouse
Kandidat1=Mickey Mouse
Kandidat2=Donald Duck
LimitKandidat2=2-3
Kandidat3=Dagobert Duck
Kandidat4=Goofy
```

## Special Thanks To

[![Bugsnag logo](Resources/bugsnag-logo.png "Bugsnag")](https://www.bugsnag.com/open-source)

for providing error reporting.
