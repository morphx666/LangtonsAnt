Imports System.Threading

Public Class FormMain
    Private bmp As DirectBitmap
    Private repaintThread As Thread
    Private algoThread As Thread
    Private grid()() As Boolean
    Private gridSize As Size
    Private antSize As Integer = 4

    Private Enum AntDirections
        Up = 0
        Right = 1
        Down = 2
        Left = 3
    End Enum
    Private antDirection As AntDirections
    Private antPosition As Point

    Private syncObject As New Object()

    Private Sub FormMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint, True)
        Me.SetStyle(ControlStyles.OptimizedDoubleBuffer, True)
        Me.SetStyle(ControlStyles.UserPaint, True)

        AddHandler Me.SizeChanged, Sub() InitBitmap()

        InitBitmap()

        repaintThread = New Thread(Sub()
                                       Do
                                           Thread.Sleep(30)
                                           Me.Invalidate()
                                       Loop
                                   End Sub) With {
                                .IsBackground = True
                             }
        repaintThread.Start()

        algoThread = New Thread(AddressOf DoLangtonsAnt) With {
            .IsBackground = True}
        algoThread.Start()
    End Sub

    Private Sub InitBitmap()
        SyncLock syncObject
            bmp = New DirectBitmap(Me.DisplayRectangle.Width, Me.DisplayRectangle.Height)
            gridSize = New Size(bmp.Width, bmp.Height)
            ReDim grid(gridSize.Width - 1)
            For x As Integer = 0 To gridSize.Width - 1
                ReDim grid(x)(gridSize.Height - 1)

                For y As Integer = 0 To gridSize.Height - 1
                    bmp.Pixel(x, y) = Color.White
                    grid(x)(y) = True
                Next
            Next

            antDirection = AntDirections.Up
            antPosition = New Point(gridSize.Width \ 2, gridSize.Height \ 2)
            NormalizeAntPosition()
        End SyncLock
    End Sub

    Private Sub Turn90Right()
        antDirection += 1
        If antDirection > AntDirections.Left Then antDirection = AntDirections.Up
    End Sub

    Private Sub Turn90Left()
        antDirection -= 1
        If antDirection < AntDirections.Up Then antDirection = AntDirections.Left
    End Sub

    Private Sub NormalizeAntPosition()
        antPosition.X -= antPosition.X Mod antSize
        antPosition.Y -= antPosition.Y Mod antSize
    End Sub

    Private Sub MoveForward()
        Select Case antDirection
            Case AntDirections.Up : antPosition.Y -= antSize
            Case AntDirections.Right : antPosition.X += antSize
            Case AntDirections.Down : antPosition.Y += antSize
            Case AntDirections.Left : antPosition.X -= antSize
        End Select

        If antPosition.X < 0 Then
            antPosition.X = gridSize.Width - 1
        ElseIf antPosition.X >= gridSize.Width Then
            antPosition.X = 0
        End If

        If antPosition.Y < 0 Then
            antPosition.Y = gridSize.Height - 1
        ElseIf antPosition.Y >= gridSize.Height Then
            antPosition.Y = 0
        End If

        NormalizeAntPosition()
    End Sub

    Private Sub DoLangtonsAnt()
        Do
            SyncLock syncObject
                For i As Integer = 0 To 10 - 1
                    If grid(antPosition.X)(antPosition.Y) Then
                        Turn90Right()
                    Else
                        Turn90Left()
                    End If
                    grid(antPosition.X)(antPosition.Y) = Not grid(antPosition.X)(antPosition.Y)

                    bmp.FillRectangle(If(grid(antPosition.X)(antPosition.Y), Color.White, Color.Black),
                                       New Rectangle(antPosition.X, antPosition.Y,
                                                     antSize, antSize))

                    MoveForward()
                Next
            End SyncLock

            Thread.Sleep(5)
        Loop
    End Sub

    Private Sub FormMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        Dim g As Graphics = e.Graphics

        g.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
        SyncLock syncObject
            g.DrawImageUnscaled(bmp.Bitmap, 0, 0)
        End SyncLock

        If antSize > 3 Then
            Using p As New Pen(Color.FromArgb(128, Color.Gray))
                For x As Integer = 0 To Me.DisplayRectangle.Width - 1 Step antSize
                    g.DrawLine(p, x, 0, x, Me.DisplayRectangle.Height - 1)
                Next
                For y As Integer = 0 To Me.DisplayRectangle.Height - 1 Step antSize
                    g.DrawLine(p, 0, y, Me.DisplayRectangle.Width - 1, y)
                Next
            End Using
        End If

        g.FillRectangle(Brushes.Red, New Rectangle(antPosition.X, antPosition.Y, antSize, antSize))
    End Sub
End Class
