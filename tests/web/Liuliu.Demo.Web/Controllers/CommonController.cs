﻿// -----------------------------------------------------------------------
//  <copyright file="CommonController.cs" company="OSharp开源团队">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>郭明锋</last-editor>
//  <last-date>2018-06-27 4:50</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OSharp.AspNetCore;
using OSharp.AspNetCore.Mvc;
using OSharp.Collections;
using OSharp.Core;
using OSharp.Core.Modules;
using OSharp.Core.Packs;
using OSharp.Drawing;
using OSharp.Entity;
using OSharp.Reflection;


namespace Liuliu.Demo.Web.Controllers
{
    [Description("网站-通用")]
    [ModuleInfo(Order = 3)]
    public class CommonController : ApiController
    {
        /// <summary>
        /// 获取验证码图片
        /// </summary>
        /// <returns>验证码图片文件</returns>
        [HttpGet]
        [ModuleInfo]
        [Description("验证码")]
        public IActionResult VerifyCode()
        {
            ValidateCoder coder = new ValidateCoder()
            {
                RandomColor = true,
                RandomItalic = true,
                RandomLineCount = 7,
                RandomPointPercent = 10,
                RandomPosition = true
            };
            Bitmap bitmap = coder.CreateImage(4, out string code);
            VerifyCodeHandler.SetCode(code);
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Jpeg);
                return File(ms.ToArray(), @"image/jpeg");
            }
        }

        /// <summary>
        /// 验证验证码的无效性
        /// </summary>
        /// <param name="code">验证码字符串</param>
        /// <returns>是否无效</returns>
        [ModuleInfo]
        [Description("验证验证码的无效性")]
        public bool CheckCodeInvalid(string code)
        {
            return !VerifyCodeHandler.CheckCode(code);
        }

        /// <summary>
        /// 获取系统信息
        /// </summary>
        /// <returns>系统信息</returns>
        [HttpGet]
        [ModuleInfo]
        [Description("系统信息")]
        public IActionResult SystemInfo()
        {
            IServiceProvider provider = HttpContext.RequestServices;

            dynamic info = new ExpandoObject();
            OSharpPackManager packManager = provider.GetService<OSharpPackManager>();
            info.Packs = packManager.SourcePacks.OrderBy(m => m.Level).ThenBy(m => m.Order).ThenBy(m => m.GetType().FullName).Select(m => new
            {
                m.GetType().Name,
                Class = m.GetType().FullName,
                Level = m.Level.ToString(),
                m.Order,
                m.IsEnabled
            }).ToList();

            string version = Assembly.GetExecutingAssembly().GetProductVersion();

            MvcOptions mvcOps = provider.GetService<IOptions<MvcOptions>>().Value;

            info.Lines = new List<string>()
            {
                "WebApi 数据服务已启动",
                $"版本号：{version}",
                $"数据连接：{provider.GetOSharpOptions().GetDbContextOptions(typeof(DefaultDbContext)).ConnectionString}",
                $"MvcFilters：\r\n{mvcOps.Filters.ExpandAndToString(m => $"{m.ToString()}-{m.GetHashCode()}", "\r\n")}"
            };

            return Json(info);
        }
    }
}