<%@ WebHandler Language="VB" Class="makeThumbnail" %>

'Imports System
'Imports System.Web
Imports System.Drawing

Public Class makeThumbnail : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim s As New System.IO.MemoryStream
        Dim currentContext As HttpContext = HttpContext.Current
        ''Dim imgPath As String = CType(currentContext.Session.Item("imgPath"), String)             ' Server.MapPathが使えるので使わないようにする 2018/05/30 Kino
        Dim sd As String = CType(currentContext.Session.Item("SD"), String)
        Dim imgPath As String = IO.Path.Combine(currentContext.Server.MapPath(sd), "img")
        imgPath = imgPath.Replace("\api", "")                                                         ' /apiフォルダに振り分けたため、それを除去する
        Dim strFileName As String = CType(currentContext.Request.QueryString("img"), String)
        If strFileName Is Nothing Then strFileName = "mainpic.png"
        Dim strFiles As String = IO.Path.Combine(imgPath, strFileName)
        Dim thumbWidth As Integer = CType(currentContext.Request.QueryString("w"), String)          ' クエリ
        Dim thumbHeight As Integer = CType(currentContext.Request.QueryString("h"), String)         ' クエリ
        Dim imgType As String = CType(currentContext.Request.QueryString("TY"), String)             ' クエリ
        ''Dim imgDate As String = CType(currentContext.Request.QueryString("dt"), String)           '文字を入れる場合
        ''Dim fnt As New Font("MS UI Gothic", 10, FontStyle.Bold)                                   '文字を入れる場合
        ''Dim tm As String = CType(currentContext.Session.Item("tm"), String)

        If strFileName = Nothing Then Exit Sub
        Dim instance As System.Drawing.Image = Nothing

        Try

            If strFileName = "None" Then
                instance = New Bitmap(thumbWidth, thumbHeight)
            Else
                instance = Image.FromFile(strFiles)
                If thumbHeight = 0 Or thumbWidth = 0 Then
                    thumbHeight = instance.Height
                    thumbWidth = instance.Width
                End If
            End If

            Using thumbnail As Bitmap = New Bitmap(thumbWidth, thumbHeight)
                Using g As Graphics = Graphics.FromImage(thumbnail)
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                    g.DrawImage(instance, 0, 0, thumbWidth, thumbHeight)
                    ''If strFileName = "None" Then
                    ''    g.DrawString("--", fnt, Brushes.Black, (thumbWidth - fnt.Size) * 0.5, (thumbHeight - fnt.Height) * 0.5)                                       '文字を入れる場合
                    ''End If
                End Using

                Dim contType As String = Nothing
                Dim imgFormat As System.Drawing.Imaging.ImageFormat
                If imgType = "JPG" Then
                    contType = "image/jpeg"
                    imgFormat = Imaging.ImageFormat.Jpeg
                Else
                    contType = "image/png"
                    imgFormat = Imaging.ImageFormat.Png
                End If

                'context.Response.ContentType = "image/png"
                ' ''instance.Save(s, Imaging.ImageFormat.Png) これでは？ 2012/06/06
                'thumbnail.Save(s, Imaging.ImageFormat.Png)

                context.Response.ContentType = contType
                thumbnail.Save(s, imgFormat)

                'context.Response.Flush()
                'context.Response.TransmitFile(strFiles)

                context.Response.Cache.SetExpires(DateTime.Parse("2000/01/01 00:00:00"))

                context.Response.BinaryWrite(s.ToArray())
            End Using
            
        Catch ex As Exception

        Finally

            instance.Dispose()

            ''context.Response.End()                                            '' これは、TreadAbortExceptionが発生する
            HttpContext.Current.ApplicationInstance.CompleteRequest()
            context.Response.Flush()
            context.Response.SuppressContent = True                             ' 余計なごみ(HTML関連)がついてしまうのを回避する　Flushと一緒に利用
            s.Dispose()
            
        End Try
            
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class