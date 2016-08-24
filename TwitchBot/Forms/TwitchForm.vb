﻿Imports System.ComponentModel
Imports System.Data.SQLite
Imports System.Threading

Public Class TwitchForm

    Public twitchUser As String
    Public twitchOAuth As String
    Public twitchChannel As String

    Public data_source As String = Application.StartupPath & "\Data\TwitchBot.sqlite"
    Public SQLconnect As SQLiteConnection = New SQLiteConnection("Data Source=" & data_source)

    Private commandThread As Thread

    Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        SQLconnect.ConnectionString = "Data Source=" & data_source & ";"
        SQLconnect.Open()

    End Sub

    Private Sub TwitchForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If My.Settings.TwitchRemember = True Then
            IRCClient.IrcClient("irc.twitch.tv", 6667, My.Settings.TwitchUser, My.Settings.TwitchOAuth)
            IRCClient.joinRoom(My.Settings.TwitchChannel)
        Else
            IRCClient.IrcClient("irc.twitch.tv", 6667, twitchUser, twitchOAuth)
            IRCClient.joinRoom(twitchChannel)
        End If

        IRCClient.sendChatMessage("TwitchBot is alive!")

        ' New Thread
        commandThread = New Thread(AddressOf StartParam)
        commandThread.IsBackground = True
        commandThread.Start()

    End Sub

    Public Sub updateFormTitle(title As String)

        Me.Text = title

    End Sub

    Sub StartParam()

        While True

            Try

                Dim message As String = IRCClient.readMessage()
                AddtoList(message)

                ' Verify bot is still active with Twitch
                If message = "PING :tmi.twitch.tv" Then
                    sendIrcMessage("PONG :tmi.twitch.tv")
                    AddtoList("PONG :tmi.twitch.tv")
                End If

                If message.Contains(":") Then
                    Dim cleanfunc As String = message.Substring(message.LastIndexOf(":") + 1)

                    If (cleanfunc.Contains("!")) Then

                        ' Database Checks
                        Dim SQLcommand1 As SQLiteCommand

                        SQLcommand1 = SQLconnect.CreateCommand
                        SQLcommand1.CommandText = "SELECT * FROM twitchbot_commands WHERE command = @command"

                        Dim commandstring As SQLiteParameter = New SQLiteParameter("@command")
                        SQLcommand1.Parameters.Add(commandstring)
                        commandstring.Value = message

                        Dim sqlreader1 As SQLiteDataReader = SQLcommand1.ExecuteReader()

                        While sqlreader1.Read()
                            IRCClient.sendChatMessage(sqlreader1("value"))
                        End While

                        SQLcommand1.Dispose()

                    End If

                End If

            Catch ex As Exception

                ' Ignore

            End Try

        End While

    End Sub

    Private Delegate Sub stringDelegate(s As String)
    Private Sub AddtoList(s As String)
        If TwitchData1.InvokeRequired Then
            Dim sd As New stringDelegate(AddressOf AddtoList)
            Me.Invoke(sd, New Object() {s})
        Else
            TwitchData1.Items.Add(s)
        End If
    End Sub

    Private Sub DeleteAccount1_Click(sender As Object, e As EventArgs) Handles DeleteAccount1.Click

        My.Settings.BotName = "TwitchBot"
        My.Settings.TwitchUser = ""
        My.Settings.TwitchOAuth = ""
        My.Settings.TwitchChannel = ""
        My.Settings.TwitchRemember = False

        My.Settings.Save()

    End Sub

    Private Sub SetBotName1_Click(sender As Object, e As EventArgs) Handles SetBotName1.Click

        Dim myBotName As New BotName
        myBotName.Show()

    End Sub

    Private Sub Exit1_Click(sender As Object, e As EventArgs) Handles Exit1.Click

        SQLconnect.Close()
        Application.Exit()

    End Sub

    Private Sub TwitchForm_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing

        SQLconnect.Close()
        Application.Exit()

    End Sub
End Class