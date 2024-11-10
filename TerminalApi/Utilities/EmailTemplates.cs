namespace TerminalApi.Utilities
{
    public static class EmailTemplates
    {
        public static string ResetPassword = @"<!DOCTYPE html>

<html
  lang=""en""
  xmlns:o=""urn:schemas-microsoft-com:office:office""
  xmlns:v=""urn:schemas-microsoft-com:vml""
>
  <head>
    <title></title>
    <meta content=""text/html; charset=utf-8"" http-equiv=""Content-Type"" />
    <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
    <!--[if mso
      ]><xml
        ><o:OfficeDocumentSettings
          ><o:PixelsPerInch>96</o:PixelsPerInch
          ><o:AllowPNG /></o:OfficeDocumentSettings></xml
    ><![endif]-->
    <!--[if !mso]><!-->
    <!--<![endif]-->
    <style>
      * {
        box-sizing: border-box;
      }

      body {
        margin: 0;
        padding: 0;
      }

      a[x-apple-data-detectors] {
        color: inherit !important;
        text-decoration: inherit !important;
      }

      #MessageViewBody a {
        color: inherit;
        text-decoration: none;
      }

      p {
        line-height: inherit;
      }

      .desktop_hide,
      .desktop_hide table {
        mso-hide: all;
        display: none;
        max-height: 0px;
        overflow: hidden;
      }

      .image_block img + div {
        display: none;
      }

      sup,
      sub {
        font-size: 75%;
        line-height: 0;
      }

      @media (max-width: 620px) {
        .desktop_hide table.icons-inner,
        .social_block.desktop_hide .social-table {
          display: inline-block !important;
        }

        .icons-inner {
          text-align: center;
        }

        .icons-inner td {
          margin: 0 auto;
        }

        .mobile_hide {
          display: none;
        }

        .row-content {
          width: 100% !important;
        }

        .stack .column {
          width: 100%;
          display: block;
        }

        .mobile_hide {
          min-height: 0;
          max-height: 0;
          max-width: 0;
          overflow: hidden;
          font-size: 0px;
        }

        .desktop_hide,
        .desktop_hide table {
          display: table !important;
          max-height: none !important;
        }
      }
    </style>
    <!--[if mso
      ]><style>
        sup,
        sub {
          font-size: 100% !important;
        }
        sup {
          mso-text-raise: 10%;
        }
        sub {
          mso-text-raise: -10%;
        }
      </style>
    <![endif]-->
  </head>
  <body
    class=""body""
    style=""
      margin: 0;
      background-color: #091548;
      padding: 0;
      -webkit-text-size-adjust: none;
      text-size-adjust: none;
    ""
  >
    <table
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""nl-container""
      role=""presentation""
      style=""
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        background-color: #091548;
      ""
      width=""100%""
    >
      <tbody>
        <tr>
          <td>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-1""
              role=""presentation""
              style=""
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
                background-color: #091548;
                background-image: url('images/background_2.png');
                background-position: center top;
                background-repeat: repeat;
              ""
              width=""100%""
            >
              <tbody>
                <tr>
                  <td>
                    <table
                      align=""center""
                      border=""0""
                      cellpadding=""0""
                      cellspacing=""0""
                      class=""row-content stack""
                      role=""presentation""
                      style=""
                        mso-table-lspace: 0pt;
                        mso-table-rspace: 0pt;
                        color: #000000;
                        width: 600px;
                        margin: 0 auto;
                      ""
                      width=""600""
                    >
                      <tbody>
                        <tr>
                          <td
                            class=""column column-1""
                            style=""
                              mso-table-lspace: 0pt;
                              mso-table-rspace: 0pt;
                              font-weight: 400;
                              text-align: left;
                              padding-bottom: 15px;
                              padding-left: 10px;
                              padding-right: 10px;
                              padding-top: 5px;
                              vertical-align: top;
                              border-top: 0px;
                              border-right: 0px;
                              border-bottom: 0px;
                              border-left: 0px;
                            ""
                            width=""100%""
                          >
                            <div
                              class=""spacer_block block-1""
                              style=""
                                height: 8px;
                                line-height: 8px;
                                font-size: 1px;
                              ""
                            >
                               
                            </div>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""image_block block-2""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    width: 100%;
                                    padding-right: 0px;
                                    padding-left: 0px;
                                  ""
                                >
                                  <div
                                    align=""center""
                                    class=""alignment""
                                    style=""line-height: 10px""
                                  >
                                    <div style=""max-width: 232px"">
                                      <img
                                        alt=""Main Image""
                                        height=""auto""
                                        src=""images/header3.png""
                                        style=""
                                          display: block;
                                          height: auto;
                                          border: 0;
                                          width: 100%;
                                        ""
                                        title=""Main Image""
                                        width=""232""
                                      />
                                    </div>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""paragraph_block block-3""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div
                                    style=""
                                      color: #ffffff;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 26px;
                                      line-height: 120%;
                                      text-align: center;
                                      mso-line-height-alt: 31.2px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      <strong>Dans la sauce</strong>
                                    </p>
                                    <p
                                      style=""
                                        margin: 0;
                                        word-break: break-word;
                                        font-size: 20px;
                                      ""
                                    >
                                      <strong
                                        >Réinitialisez votre Mot de
                                        passe</strong
                                      >
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""5""
                              cellspacing=""0""
                              class=""paragraph_block block-4""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div
                                    style=""
                                      color: #ffffff;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 14px;
                                      line-height: 150%;
                                      text-align: center;
                                      mso-line-height-alt: 21px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      Cliquez le lien suivant 
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""button_block block-5""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 20px;
                                    padding-left: 15px;
                                    padding-right: 15px;
                                    padding-top: 20px;
                                    text-align: center;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <a
                                      href=""{{link}}""
                                      style=""color: #091548""
                                      target=""_blank""
                                      >><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  href=""http://www.example.com/""  style=""height:40px;width:272px;v-text-anchor:middle;"" arcsize=""60%"" fillcolor=""#ffffff"">
<v:stroke dashstyle=""Solid"" weight=""0px"" color=""#ffffff""/>
<w:anchorlock/>
<v:textbox inset=""0px,0px,0px,0px"">
<center dir=""false"" style=""color:#091548;font-family:sans-serif;font-size:15px"">
<![endif]-->
                                      <div
                                        style=""
                                          background-color: #ffffff;
                                          border-bottom: 0px solid transparent;
                                          border-left: 0px solid transparent;
                                          border-radius: 24px;
                                          border-right: 0px solid transparent;
                                          border-top: 0px solid transparent;
                                          color: #091548;
                                          display: inline-block;
                                          font-family: 'Varela Round',
                                            'Trebuchet MS', Helvetica,
                                            sans-serif;
                                          font-size: 15px;
                                          font-weight: undefined;
                                          mso-border-alt: none;
                                          padding-bottom: 5px;
                                          padding-top: 5px;
                                          text-align: center;
                                          text-decoration: none;
                                          width: auto;
                                          word-break: keep-all;
                                        ""
                                      >
                                        <span
                                          style=""
                                            word-break: break-word;
                                            padding-left: 25px;
                                            padding-right: 25px;
                                            font-size: 15px;
                                            display: inline-block;
                                            letter-spacing: normal;
                                          ""
                                          ><span style=""word-break: break-word""
                                            ><span
                                              data-mce-style=""""
                                              style=""
                                                word-break: break-word;
                                                line-height: 30px;
                                              ""
                                              ><strong
                                                >Réinitialisez votre Mot de
                                                passe</strong
                                              ></span
                                            ></span
                                          ></span
                                        >
                                      </div>
                                      <!--[if mso]></center></v:textbox></v:roundrect><![endif]-->
                                    </a>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""divider_block block-6""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-left: 10px;
                                    padding-right: 10px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <table
                                      border=""0""
                                      cellpadding=""0""
                                      cellspacing=""0""
                                      role=""presentation""
                                      style=""
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                      ""
                                      width=""60%""
                                    >
                                      <tr>
                                        <td
                                          class=""divider_inner""
                                          style=""
                                            font-size: 1px;
                                            line-height: 1px;
                                            border-top: 1px solid #5a6ba8;
                                          ""
                                        >
                                          <span style=""word-break: break-word""
                                            > </span
                                          >
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""paragraph_block block-7""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 10px;
                                    padding-left: 25px;
                                    padding-right: 25px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div
                                    style=""
                                      color: #7f96ef;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 14px;
                                      line-height: 150%;
                                      text-align: center;
                                      mso-line-height-alt: 21px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      Si vous n'êtes pas à l'origine de cette
                                      demande
                                    </p>
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      ignorez ce mail
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <div
                              class=""spacer_block block-8""
                              style=""
                                height: 30px;
                                line-height: 30px;
                                font-size: 1px;
                              ""
                            >
                               
                            </div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-2""
              role=""presentation""
              style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt""
              width=""100%""
            >
              <tbody>
                <tr>
                  <td>
                    <table
                      align=""center""
                      border=""0""
                      cellpadding=""0""
                      cellspacing=""0""
                      class=""row-content stack""
                      role=""presentation""
                      style=""
                        mso-table-lspace: 0pt;
                        mso-table-rspace: 0pt;
                        color: #000000;
                        width: 600px;
                        margin: 0 auto;
                      ""
                      width=""600""
                    >
                      <tbody>
                        <tr>
                          <td
                            class=""column column-1""
                            style=""
                              mso-table-lspace: 0pt;
                              mso-table-rspace: 0pt;
                              font-weight: 400;
                              text-align: left;
                              padding-bottom: 15px;
                              padding-left: 10px;
                              padding-right: 10px;
                              padding-top: 15px;
                              vertical-align: top;
                              border-top: 0px;
                              border-right: 0px;
                              border-bottom: 0px;
                              border-left: 0px;
                            ""
                            width=""100%""
                          >
                            <table
                              border=""0""
                              cellpadding=""5""
                              cellspacing=""0""
                              class=""image_block block-1""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div
                                    align=""center""
                                    class=""alignment""
                                    style=""line-height: 10px""
                                  >
                                    <div style=""max-width: 145px"">
                                      <img
                                        alt=""Your Logo""
                                        height=""auto""
                                        src=""images/logo.png""
                                        style=""
                                          display: block;
                                          height: auto;
                                          border: 0;
                                          width: 100%;
                                        ""
                                        title=""Dans la sauce""
                                        width=""145""
                                      />
                                    </div>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""divider_block block-2""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-left: 10px;
                                    padding-right: 10px;
                                    padding-top: 15px;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <table
                                      border=""0""
                                      cellpadding=""0""
                                      cellspacing=""0""
                                      role=""presentation""
                                      style=""
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                      ""
                                      width=""60%""
                                    >
                                      <tr>
                                        <td
                                          class=""divider_inner""
                                          style=""
                                            font-size: 1px;
                                            line-height: 1px;
                                            border-top: 1px solid #5a6ba8;
                                          ""
                                        >
                                          <span style=""word-break: break-word""
                                            > </span
                                          >
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""10""
                              cellspacing=""0""
                              class=""social_block block-3""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div align=""center"" class=""alignment""></div>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-3""
              role=""presentation""
              style=""
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
                background-color: #ffffff;
              ""
              width=""100%""
            ></table>
          </td>
        </tr>
      </tbody>
    </table>
    <!-- End -->
  </body>
</html>
";
        public static string ConfirmeMail = @"<!DOCTYPE html>

<html
  lang=""en""
  xmlns:o=""urn:schemas-microsoft-com:office:office""
  xmlns:v=""urn:schemas-microsoft-com:vml""
>
  <head>
    <title></title>
    <meta content=""text/html; charset=utf-8"" http-equiv=""Content-Type"" />
    <meta content=""width=device-width, initial-scale=1.0"" name=""viewport"" />
    <!--[if mso
      ]><xml
        ><o:OfficeDocumentSettings
          ><o:PixelsPerInch>96</o:PixelsPerInch
          ><o:AllowPNG /></o:OfficeDocumentSettings></xml
    ><![endif]-->
    <!--[if !mso]><!-->
    <!--<![endif]-->
    <style>
      * {
        box-sizing: border-box;
      }

      body {
        margin: 0;
        padding: 0;
      }

      a[x-apple-data-detectors] {
        color: inherit !important;
        text-decoration: inherit !important;
      }

      #MessageViewBody a {
        color: inherit;
        text-decoration: none;
      }

      p {
        line-height: inherit;
      }

      .desktop_hide,
      .desktop_hide table {
        mso-hide: all;
        display: none;
        max-height: 0px;
        overflow: hidden;
      }

      .image_block img + div {
        display: none;
      }

      sup,
      sub {
        font-size: 75%;
        line-height: 0;
      }

      @media (max-width: 620px) {
        .desktop_hide table.icons-inner,
        .social_block.desktop_hide .social-table {
          display: inline-block !important;
        }

        .icons-inner {
          text-align: center;
        }

        .icons-inner td {
          margin: 0 auto;
        }

        .mobile_hide {
          display: none;
        }

        .row-content {
          width: 100% !important;
        }

        .stack .column {
          width: 100%;
          display: block;
        }

        .mobile_hide {
          min-height: 0;
          max-height: 0;
          max-width: 0;
          overflow: hidden;
          font-size: 0px;
        }

        .desktop_hide,
        .desktop_hide table {
          display: table !important;
          max-height: none !important;
        }
      }
    </style>
    <!--[if mso
      ]><style>
        sup,
        sub {
          font-size: 100% !important;
        }
        sup {
          mso-text-raise: 10%;
        }
        sub {
          mso-text-raise: -10%;
        }
      </style>
    <![endif]-->
  </head>
  <body
    class=""body""
    style=""
      margin: 0;
      background-color: #091548;
      padding: 0;
      -webkit-text-size-adjust: none;
      text-size-adjust: none;
    ""
  >
    <table
      border=""0""
      cellpadding=""0""
      cellspacing=""0""
      class=""nl-container""
      role=""presentation""
      style=""
        mso-table-lspace: 0pt;
        mso-table-rspace: 0pt;
        background-color: #091548;
      ""
      width=""100%""
    >
      <tbody>
        <tr>
          <td>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-1""
              role=""presentation""
              style=""
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
                background-color: #091548;
                background-image: url('images/background_2.png');
                background-position: center top;
                background-repeat: repeat;
              ""
              width=""100%""
            >
              <tbody>
                <tr>
                  <td>
                    <table
                      align=""center""
                      border=""0""
                      cellpadding=""0""
                      cellspacing=""0""
                      class=""row-content stack""
                      role=""presentation""
                      style=""
                        mso-table-lspace: 0pt;
                        mso-table-rspace: 0pt;
                        color: #000000;
                        width: 600px;
                        margin: 0 auto;
                      ""
                      width=""600""
                    >
                      <tbody>
                        <tr>
                          <td
                            class=""column column-1""
                            style=""
                              mso-table-lspace: 0pt;
                              mso-table-rspace: 0pt;
                              font-weight: 400;
                              text-align: left;
                              padding-bottom: 15px;
                              padding-left: 10px;
                              padding-right: 10px;
                              padding-top: 5px;
                              vertical-align: top;
                              border-top: 0px;
                              border-right: 0px;
                              border-bottom: 0px;
                              border-left: 0px;
                            ""
                            width=""100%""
                          >
                            <div
                              class=""spacer_block block-1""
                              style=""
                                height: 8px;
                                line-height: 8px;
                                font-size: 1px;
                              ""
                            >
                               
                            </div>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""image_block block-2""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    width: 100%;
                                    padding-right: 0px;
                                    padding-left: 0px;
                                  ""
                                >
                                  <div
                                    align=""center""
                                    class=""alignment""
                                    style=""line-height: 10px""
                                  >
                                    <div style=""max-width: 232px"">
                                      <img
                                        alt=""Main Image""
                                        height=""auto""
                                        src=""images/header3.png""
                                        style=""
                                          display: block;
                                          height: auto;
                                          border: 0;
                                          width: 100%;
                                        ""
                                        title=""Main Image""
                                        width=""232""
                                      />
                                    </div>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""paragraph_block block-3""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div
                                    style=""
                                      color: #ffffff;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 26px;
                                      line-height: 120%;
                                      text-align: center;
                                      mso-line-height-alt: 31.2px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      <strong>Dans la sauce</strong>
                                    </p>
                                    <p
                                      style=""
                                        margin: 0;
                                        word-break: break-word;
                                        font-size: 20px;
                                      ""
                                    >
                                      <strong
                                        >Confirmez votre compte</strong
                                      >
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""5""
                              cellspacing=""0""
                              class=""paragraph_block block-4""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div
                                    style=""
                                      color: #ffffff;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 14px;
                                      line-height: 150%;
                                      text-align: center;
                                      mso-line-height-alt: 21px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      Cliquez le lien suivant 
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""button_block block-5""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 20px;
                                    padding-left: 15px;
                                    padding-right: 15px;
                                    padding-top: 20px;
                                    text-align: center;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <a
                                      href=""{{link}}""
                                      style=""color: #091548""
                                      target=""_blank""
                                      >><!--[if mso]>
<v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word""  href=""http://www.example.com/""  style=""height:40px;width:272px;v-text-anchor:middle;"" arcsize=""60%"" fillcolor=""#ffffff"">
<v:stroke dashstyle=""Solid"" weight=""0px"" color=""#ffffff""/>
<w:anchorlock/>
<v:textbox inset=""0px,0px,0px,0px"">
<center dir=""false"" style=""color:#091548;font-family:sans-serif;font-size:15px"">
<![endif]-->
                                      <div
                                        style=""
                                          background-color: #ffffff;
                                          border-bottom: 0px solid transparent;
                                          border-left: 0px solid transparent;
                                          border-radius: 24px;
                                          border-right: 0px solid transparent;
                                          border-top: 0px solid transparent;
                                          color: #091548;
                                          display: inline-block;
                                          font-family: 'Varela Round',
                                            'Trebuchet MS', Helvetica,
                                            sans-serif;
                                          font-size: 15px;
                                          font-weight: undefined;
                                          mso-border-alt: none;
                                          padding-bottom: 5px;
                                          padding-top: 5px;
                                          text-align: center;
                                          text-decoration: none;
                                          width: auto;
                                          word-break: keep-all;
                                        ""
                                      >
                                        <span
                                          style=""
                                            word-break: break-word;
                                            padding-left: 25px;
                                            padding-right: 25px;
                                            font-size: 15px;
                                            display: inline-block;
                                            letter-spacing: normal;
                                          ""
                                          ><span style=""word-break: break-word""
                                            ><span
                                              data-mce-style=""""
                                              style=""
                                                word-break: break-word;
                                                line-height: 30px;
                                              ""
                                              ><strong
                                                >Réinitialisez votre Mot de
                                                passe</strong
                                              ></span
                                            ></span
                                          ></span
                                        >
                                      </div>
                                      <!--[if mso]></center></v:textbox></v:roundrect><![endif]-->
                                    </a>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""divider_block block-6""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-left: 10px;
                                    padding-right: 10px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <table
                                      border=""0""
                                      cellpadding=""0""
                                      cellspacing=""0""
                                      role=""presentation""
                                      style=""
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                      ""
                                      width=""60%""
                                    >
                                      <tr>
                                        <td
                                          class=""divider_inner""
                                          style=""
                                            font-size: 1px;
                                            line-height: 1px;
                                            border-top: 1px solid #5a6ba8;
                                          ""
                                        >
                                          <span style=""word-break: break-word""
                                            > </span
                                          >
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""paragraph_block block-7""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                                word-break: break-word;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 10px;
                                    padding-left: 25px;
                                    padding-right: 25px;
                                    padding-top: 10px;
                                  ""
                                >
                                  <div
                                    style=""
                                      color: #7f96ef;
                                      font-family: 'Varela Round',
                                        'Trebuchet MS', Helvetica, sans-serif;
                                      font-size: 14px;
                                      line-height: 150%;
                                      text-align: center;
                                      mso-line-height-alt: 21px;
                                    ""
                                  >
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      Si vous n'êtes pas à l'origine de cette
                                      demande
                                    </p>
                                    <p
                                      style=""margin: 0; word-break: break-word""
                                    >
                                      ignorez ce mail
                                    </p>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <div
                              class=""spacer_block block-8""
                              style=""
                                height: 30px;
                                line-height: 30px;
                                font-size: 1px;
                              ""
                            >
                               
                            </div>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-2""
              role=""presentation""
              style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt""
              width=""100%""
            >
              <tbody>
                <tr>
                  <td>
                    <table
                      align=""center""
                      border=""0""
                      cellpadding=""0""
                      cellspacing=""0""
                      class=""row-content stack""
                      role=""presentation""
                      style=""
                        mso-table-lspace: 0pt;
                        mso-table-rspace: 0pt;
                        color: #000000;
                        width: 600px;
                        margin: 0 auto;
                      ""
                      width=""600""
                    >
                      <tbody>
                        <tr>
                          <td
                            class=""column column-1""
                            style=""
                              mso-table-lspace: 0pt;
                              mso-table-rspace: 0pt;
                              font-weight: 400;
                              text-align: left;
                              padding-bottom: 15px;
                              padding-left: 10px;
                              padding-right: 10px;
                              padding-top: 15px;
                              vertical-align: top;
                              border-top: 0px;
                              border-right: 0px;
                              border-bottom: 0px;
                              border-left: 0px;
                            ""
                            width=""100%""
                          >
                            <table
                              border=""0""
                              cellpadding=""5""
                              cellspacing=""0""
                              class=""image_block block-1""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div
                                    align=""center""
                                    class=""alignment""
                                    style=""line-height: 10px""
                                  >
                                    <div style=""max-width: 145px"">
                                      <img
                                        alt=""Your Logo""
                                        height=""auto""
                                        src=""images/logo.png""
                                        style=""
                                          display: block;
                                          height: auto;
                                          border: 0;
                                          width: 100%;
                                        ""
                                        title=""Dans la sauce""
                                        width=""145""
                                      />
                                    </div>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""0""
                              cellspacing=""0""
                              class=""divider_block block-2""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td
                                  class=""pad""
                                  style=""
                                    padding-bottom: 15px;
                                    padding-left: 10px;
                                    padding-right: 10px;
                                    padding-top: 15px;
                                  ""
                                >
                                  <div align=""center"" class=""alignment"">
                                    <table
                                      border=""0""
                                      cellpadding=""0""
                                      cellspacing=""0""
                                      role=""presentation""
                                      style=""
                                        mso-table-lspace: 0pt;
                                        mso-table-rspace: 0pt;
                                      ""
                                      width=""60%""
                                    >
                                      <tr>
                                        <td
                                          class=""divider_inner""
                                          style=""
                                            font-size: 1px;
                                            line-height: 1px;
                                            border-top: 1px solid #5a6ba8;
                                          ""
                                        >
                                          <span style=""word-break: break-word""
                                            > </span
                                          >
                                        </td>
                                      </tr>
                                    </table>
                                  </div>
                                </td>
                              </tr>
                            </table>
                            <table
                              border=""0""
                              cellpadding=""10""
                              cellspacing=""0""
                              class=""social_block block-3""
                              role=""presentation""
                              style=""
                                mso-table-lspace: 0pt;
                                mso-table-rspace: 0pt;
                              ""
                              width=""100%""
                            >
                              <tr>
                                <td class=""pad"">
                                  <div align=""center"" class=""alignment""></div>
                                </td>
                              </tr>
                            </table>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
            <table
              align=""center""
              border=""0""
              cellpadding=""0""
              cellspacing=""0""
              class=""row row-3""
              role=""presentation""
              style=""
                mso-table-lspace: 0pt;
                mso-table-rspace: 0pt;
                background-color: #ffffff;
              ""
              width=""100%""
            ></table>
          </td>
        </tr>
      </tbody>
    </table>
    <!-- End -->
  </body>
</html>
";
    }
}
