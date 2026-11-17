' Assignment 3: Text Encoder / Decoder (VB.NET Windows Forms App)
' ---------------------------------------------------------------
' Paste this over the ENTIRE contents of Form1.vb (not Form1.Designer.vb).
' No controls need to be dragged onto the form: the UI is built in code.
'
' Algorithms: Caesar, Vigenere, Atbash, ROT13, Base64, Binary

Imports System.Collections.Generic
Imports System.Text

Public Class Form1

    Private cboAlgo As New ComboBox()
    Private lblKey As New Label()
    Private txtKey As New TextBox()
    Private txtInput As New TextBox()
    Private txtOutput As New TextBox()
    Private btnEncode As New Button()
    Private btnDecode As New Button()
    Private btnSwap As New Button()
    Private btnClear As New Button()

    ' ------------------------------------------------------------ UI setup
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Text Encoder / Decoder"
        Me.ClientSize = New Size(600, 490)
        Me.MinimumSize = New Size(500, 450)

        AddLabel("Algorithm:", 12, 10)
        cboAlgo.DropDownStyle = ComboBoxStyle.DropDownList
        cboAlgo.Items.AddRange(New Object() {
            "Caesar cipher", "Vigenere cipher", "Atbash cipher",
            "ROT13", "Base64", "Binary"})
        cboAlgo.SetBounds(12, 30, 576, 24)
        cboAlgo.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        AddHandler cboAlgo.SelectedIndexChanged, AddressOf UpdateKeyState
        Me.Controls.Add(cboAlgo)

        lblKey.SetBounds(12, 65, 300, 18)
        Me.Controls.Add(lblKey)
        txtKey.SetBounds(12, 85, 576, 24)
        txtKey.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(txtKey)

        AddLabel("Input text:", 12, 120)
        txtInput.Multiline = True
        txtInput.ScrollBars = ScrollBars.Vertical
        txtInput.SetBounds(12, 140, 576, 110)
        txtInput.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(txtInput)

        SetupButton(btnEncode, "Encode", 12)
        SetupButton(btnDecode, "Decode", 112)
        SetupButton(btnSwap, "Output " & FromCode(8594) & " Input", 212)
        SetupButton(btnClear, "Clear", 332)
        btnSwap.Width = 110
        AddHandler btnEncode.Click, Sub(s, ev) Process(False)
        AddHandler btnDecode.Click, Sub(s, ev) Process(True)
        AddHandler btnSwap.Click, Sub(s, ev)
                                      txtInput.Text = txtOutput.Text
                                      txtOutput.Clear()
                                  End Sub
        AddHandler btnClear.Click, Sub(s, ev)
                                       txtInput.Clear()
                                       txtOutput.Clear()
                                       txtKey.Clear()
                                   End Sub

        AddLabel("Result:", 12, 295)
        txtOutput.Multiline = True
        txtOutput.ReadOnly = True
        txtOutput.ScrollBars = ScrollBars.Vertical
        txtOutput.SetBounds(12, 315, 576, 163)
        txtOutput.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or
                           AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(txtOutput)

        cboAlgo.SelectedIndex = 0
    End Sub

    Private Sub AddLabel(text As String, x As Integer, y As Integer)
        Dim lbl As New Label()
        lbl.Text = text
        lbl.SetBounds(x, y, 300, 18)
        Me.Controls.Add(lbl)
    End Sub

    Private Sub SetupButton(btn As Button, text As String, x As Integer)
        btn.Text = text
        btn.SetBounds(x, 258, 90, 28)
        Me.Controls.Add(btn)
    End Sub

    Private Sub UpdateKeyState(sender As Object, e As EventArgs)
        Select Case cboAlgo.SelectedIndex
            Case 0
                lblKey.Text = "Shift key (whole number):"
                txtKey.Enabled = True
            Case 1
                lblKey.Text = "Keyword (letters only):"
                txtKey.Enabled = True
            Case Else
                lblKey.Text = "Key (not needed for this algorithm)"
                txtKey.Enabled = False
        End Select
    End Sub

    ' --------------------------------------------------------- Run algorithm
    Private Sub Process(decode As Boolean)
        Try
            Dim text As String = txtInput.Text
            Select Case cboAlgo.SelectedIndex
                Case 0
                    Dim shift As Integer
                    If Not Integer.TryParse(txtKey.Text, shift) Then
                        MessageBox.Show("Please enter a whole number as the shift key.",
                                        "Invalid key", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        Return
                    End If
                    txtOutput.Text = Caesar(text, shift, decode)
                Case 1
                    txtOutput.Text = Vigenere(text, txtKey.Text, decode)
                Case 2
                    txtOutput.Text = Atbash(text)
                Case 3
                    txtOutput.Text = Caesar(text, 13, decode)
                Case 4
                    txtOutput.Text = If(decode, Base64Decode(text), Base64Encode(text))
                Case 5
                    txtOutput.Text = If(decode, BinaryDecode(text), BinaryEncode(text))
            End Select
        Catch ex As Exception
            MessageBox.Show("Error: " & ex.Message, "Could not process",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' ------------------------------------------------------------- Caesar
    Private Function Caesar(text As String, shift As Integer, decode As Boolean) As String
        If decode Then shift = -shift
        Dim sb As New StringBuilder()
        For Each ch As Char In text
            If Char.IsLetter(ch) AndAlso Code(ch) < 128 Then
                Dim baseCode As Integer = If(Char.IsUpper(ch), Code("A"c), Code("a"c))
                Dim shifted As Integer = ((Code(ch) - baseCode + shift) Mod 26 + 26) Mod 26
                sb.Append(FromCode(shifted + baseCode))
            Else
                sb.Append(ch)
            End If
        Next
        Return sb.ToString()
    End Function

    ' ----------------------------------------------------------- Vigenere
    Private Function Vigenere(text As String, key As String, decode As Boolean) As String
        Dim cleanKey As New StringBuilder()
        For Each c As Char In key.ToLower()
            If c >= "a"c AndAlso c <= "z"c Then cleanKey.Append(c)
        Next
        If cleanKey.Length = 0 Then
            Throw New ArgumentException("Keyword must contain at least one letter.")
        End If

        Dim sb As New StringBuilder()
        Dim k As Integer = 0
        For Each ch As Char In text
            If Char.IsLetter(ch) AndAlso Code(ch) < 128 Then
                Dim shift As Integer = Code(cleanKey(k Mod cleanKey.Length)) - Code("a"c)
                If decode Then shift = -shift
                Dim baseCode As Integer = If(Char.IsUpper(ch), Code("A"c), Code("a"c))
                Dim shifted As Integer = ((Code(ch) - baseCode + shift) Mod 26 + 26) Mod 26
                sb.Append(FromCode(shifted + baseCode))
                k += 1
            Else
                sb.Append(ch)
            End If
        Next
        Return sb.ToString()
    End Function

    ' ------------------------------------------------------------- Atbash
    ' Symmetric: encoding and decoding are the same operation.
    Private Function Atbash(text As String) As String
        Dim sb As New StringBuilder()
        For Each ch As Char In text
            If ch >= "a"c AndAlso ch <= "z"c Then
                sb.Append(FromCode(Code("z"c) - (Code(ch) - Code("a"c))))
            ElseIf ch >= "A"c AndAlso ch <= "Z"c Then
                sb.Append(FromCode(Code("Z"c) - (Code(ch) - Code("A"c))))
            Else
                sb.Append(ch)
            End If
        Next
        Return sb.ToString()
    End Function

    ' ------------------------------------------------------------- Base64
    Private Function Base64Encode(text As String) As String
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(text))
    End Function

    Private Function Base64Decode(text As String) As String
        Return Encoding.UTF8.GetString(Convert.FromBase64String(text.Trim()))
    End Function

    ' ------------------------------------------------------------- Binary
    Private Function BinaryEncode(text As String) As String
        Dim parts As New List(Of String)
        For Each b As Byte In Encoding.UTF8.GetBytes(text)
            parts.Add(Convert.ToString(b, 2).PadLeft(8, "0"c))
        Next
        Return String.Join(" ", parts)
    End Function

    Private Function BinaryDecode(text As String) As String
        Dim chunks() As String = text.Split(New Char() {" "c, FromCode(13), FromCode(10)},
                                    StringSplitOptions.RemoveEmptyEntries)

        Dim bytes(chunks.Length - 1) As Byte
        For i As Integer = 0 To chunks.Length - 1
            bytes(i) = Convert.ToByte(chunks(i), 2)
        Next
        Return Encoding.UTF8.GetString(bytes)
    End Function

    Private Function Code(c As Char) As Integer
        Return Convert.ToInt32(c)
    End Function

    Private Function FromCode(n As Integer) As Char
        Return Convert.ToChar(n)
    End Function
End Class