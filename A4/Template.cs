using A4.DATA.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace A4
{
    public class Template
    {
        private string rootPath;
        private string indexDocumentPath;
        private DirectoryInfo shareDocumentPath;

        private string tag;
        private string content;

        private bool isEmpty;
        private bool isIndexTemplate;
        private bool hasPrivateShareTemplate;

        private List<Template> innerTemplate;

        public delegate bool ForeachATagPredicate(int _aTagIndex, ref string _content);

        public Func<Template, string, string> TemplateDocumentNameReplaceFunc;

        public static string tag_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_$.";

        public static string InternalParamsPreChars = "$$$";
        public static string TagTypeComboPreChars = "$$";

        public static string PrivateShareDocumentFolder = "Common";
        public static Template Root;

        public bool IsIndexTemplate { get { return this.isIndexTemplate; } }

        public Template()
        {
            this.isEmpty = true;
            this.isIndexTemplate = false;
            this.hasPrivateShareTemplate = false;
        }

        public Template(string _content)
        {
            this.content = _content;

            this.isEmpty = false;
            this.isIndexTemplate = true;
            this.hasPrivateShareTemplate = false;
        }

        public Template(DirectoryInfo _rootDirectoryInfo, DirectoryInfo _shareDirectoryInfo = null, Func<Template, string, string> _templateDocumentNameReplaceFunc = null)
        {
            this.innerTemplate = new List<Template>();

            this.isEmpty = false;
            this.isIndexTemplate = true;
            this.hasPrivateShareTemplate = false;

            this.shareDocumentPath = _shareDirectoryInfo;
            this.TemplateDocumentNameReplaceFunc = _templateDocumentNameReplaceFunc;

            this.InitPathInfos(_rootDirectoryInfo.Name + ".html", _rootDirectoryInfo.FullName);

            Template.Root = this;

            this.ScanDocument();
        }

        public Template(string _tag, string _rootPath, Func<Template, string, string> _templateDocumentNameReplaceFunc = null)
        {
            this.innerTemplate = new List<Template>();

            this.isEmpty = false;
            this.isIndexTemplate = false;
            this.hasPrivateShareTemplate = false;

            this.tag = _tag.TrimStart(new char[] { '{' }).TrimEnd(new char[] { '}' });

            this.TemplateDocumentNameReplaceFunc = _templateDocumentNameReplaceFunc;

            this.InitPathInfos(this.tag + ".html", _rootPath);

            this.ScanDocument();
        }

        private void InitPathInfos(string _docuemntName, string _documentFullPath)
        {
            this.indexDocumentPath = _docuemntName;

            if (this.TemplateDocumentNameReplaceFunc != null)
            {
                this.indexDocumentPath = this.TemplateDocumentNameReplaceFunc(this, this.indexDocumentPath);
            }

            if (_documentFullPath.EndsWith("\\"))
            {
                this.rootPath = _documentFullPath;
                this.indexDocumentPath = string.Format(@"{0}\{1}", _documentFullPath.TrimEnd(new char[] { '\\' }), this.indexDocumentPath);
            }
            else
            {
                this.rootPath = string.Format(@"{0}\", _documentFullPath);
                this.indexDocumentPath = string.Format(@"{0}\{1}", _documentFullPath, this.indexDocumentPath);
            }

            if (this.isIndexTemplate)
            {
                this.hasPrivateShareTemplate = Directory.Exists(string.Format(@"{0}\{1}\", this.rootPath, Template.PrivateShareDocumentFolder));
            }
        }

        private bool ScanDocument()
        {
            if (!this.isIndexTemplate)
            {
                if (!File.Exists(this.indexDocumentPath))
                {
                    return false;
                }
            }

            using (StreamReader fs = System.IO.File.OpenText(this.indexDocumentPath))
            {
                this.content = fs.ReadToEnd();
            }

            List<string> tags = Template.Tags(ref this.content);

            bool tagInStatus = false;

            foreach (string tag in tags)
            {
                Template t = new Template(tag, this.rootPath, this.TemplateDocumentNameReplaceFunc);

                this.innerTemplate.Add(t);

                tagInStatus = this + t;
            }

            return true;
        }

        public static List<Template.TagInfo.Rnd> RndTags(ref string _blockContent, ref string _content, string _keyWord, string _blockContent_ReplaceHolderTemplateTag, Func<string> _placeHolderRndGenerationFunc)
        {
            int contentIndex = _blockContent.Length - 1;

            int keyWordLength = _keyWord.Length;

            int loopIndex = 0;

            int kCount = -1;

            int blocksIndex = -1;

            string tmp1 = "";
            string tmp2 = "";

            int dotIndex = -1;

            string keyFix = "";

            loopIndex = _blockContent.IndexOf(_keyWord);

            if (loopIndex == -1) return null;

            // (return)
            // 
            List<TagInfo.Rnd> rndTags = new List<TagInfo.Rnd>();

            // ===

            List<BlockInfo> blocks = Template.Blocks(ref _content, "/* {0} */", _keyWord, _keyWord, _replaceHolderTemplateTag: string.Format(Page.ReplaceHolderTemplateTag2, string.Format("{0}-{1}", "TagInfoRnd", _blockContent_ReplaceHolderTemplateTag), "{0}"));

            blocksIndex = blocks.Count - 1;

            //if (blocksIndex == -1) return null;

            //List<Template.BlockInfo> blocks1 = Template.Blocks(_content, "<!-- {0} -->", _keyWord, _keyWord);

            while (loopIndex != -1 && loopIndex < contentIndex)
            {
                kCount++;

                //if (kCount > blocksIndex) break;

                #region Template => Content => (HTML) Column Block -> (Rnd Block)s

                var rnd = new Template.TagInfo.Rnd()
                {
                    Name = _keyWord,
                    Value = _placeHolderRndGenerationFunc(),
                    TemplateHolder = (kCount > blocksIndex) ? null : blocks[kCount]
                };

                #endregion

                dotIndex = _keyWord.IndexOf(".");

                #region Plugin (.*)

                if (dotIndex != -1)
                {
                    // Ex : "Rnd"
                    //
                    keyFix = _keyWord.Substring(dotIndex + 1);

                    switch (keyFix)
                    {
                        case "Rnd":
                            {
                                #region Rnd Block Content => replace : "$$Rnd" => rnd.Value

                                // "$$Rnd"
                                //
                                //var kewWordReplaceHolder = string.Format("{0}{1}", Template.TagTypeComboPreChars, keyFix);

                                //rnd.TemplateHolder.Value = rnd.TemplateHolder.Value.Replace(kewWordReplaceHolder, rnd.Value);

                                #endregion

                                #region blockContent => replace : *.Rnd (_keyWord) => rnd.Value

                                //tmp1 = _blockContent.Substring(0, loopIndex + 1 - 1);

                                //tmp2 = _blockContent.Substring(loopIndex + keyWordLength);

                                //_blockContent = tmp1 + rnd.Value + tmp2;

                                #endregion
                            }
                            break;
                    }
                }

                #endregion

                rndTags.Add(rnd);

                // ===

                #region loop detected

                contentIndex = _blockContent.Length - 1;

                loopIndex = _blockContent.IndexOf(_keyWord, loopIndex + 1);

                #endregion
            }

            return rndTags;
        }

        public static List<string> Tags(ref string _content, char _aTag = '{', char _bTag = '}')
        {
            List<string> tmpList = new List<string>();

            int aTagIndex = 0;
            int bTagIndex = 0;
            int loopIndex = 0;

            int contentIndex = _content.Length - 1;

            string tmpStr = "";
            char charStr;

            loopIndex = _content.IndexOf(_aTag);

            if (loopIndex == -1) return tmpList;

            loopIndex++;
            aTagIndex = loopIndex;

            while (loopIndex <= contentIndex)
            {
                charStr = Convert.ToChar(_content.Substring(loopIndex, 1));

                if (charStr == _bTag)
                {
                    bTagIndex = loopIndex;
                    tmpStr = _content.Substring(aTagIndex, bTagIndex - aTagIndex);

                    tmpList.Add(tmpStr);

                    loopIndex++;

                    continue;
                }

                // next char
                //
                if (Template.tag_chars.IndexOf(charStr) == -1)
                {
                    loopIndex = _content.IndexOf(_aTag, loopIndex + 1);

                    if (loopIndex == -1) break;

                    loopIndex++;
                    aTagIndex = loopIndex;

                    continue;
                }

                loopIndex++;
            }

            return tmpList;
        }

        public static BlockInfo BlockFirst(string _content, string _templateTag, string _aTag, string _bTag, string _searchBeforeTag = "", string _replaceHolderTemplateTag = "")
        {
            List<BlockInfo> tmp = Template.Blocks(ref _content, _templateTag, _aTag, _bTag, 0, _searchBeforeTag, _replaceHolderTemplateTag);

            if (tmp == null || tmp.Count == 0) return null;

            return tmp[0];
        }

        public static List<BlockInfo> Blocks(string _content, string _templateTag, string _aTag, string _bTag, int _targetBlockIndex = -1, string _searchBeforeTag = "", string _replaceHolderTemplateTag = "", ForeachATagPredicate _foreachATagPredicate = null)
        {
            return Template.Blocks(ref _content, _templateTag, _aTag, _bTag, _targetBlockIndex, _searchBeforeTag, _replaceHolderTemplateTag, _foreachATagPredicate);
        }

        public static List<BlockInfo> Blocks(ref string _content, string _templateTag, string _aTag, string _bTag, int _targetBlockIndex = -1, string _searchBeforeTag = "", string _replaceHolderTemplateTag = "", ForeachATagPredicate _foreachATagPredicate = null)
        {
            _aTag = string.Format(_templateTag, _aTag);
            _bTag = string.Format(_templateTag, _bTag);

            int aTagLength = _aTag.Length;
            int bTagLength = _bTag.Length;

            int aTagIndex = 0;
            int bTagIndex = 0;

            int blockIndex = -1;

            bool hasReplaceHolderTemplateTag = !String.IsNullOrEmpty(_replaceHolderTemplateTag);

            string replaceHolderTemplateTagTmp = "";

            List<BlockInfo> tag_templates = new List<BlockInfo>();

            aTagIndex = _content.IndexOf(_aTag);

            if (aTagIndex == -1) return tag_templates;

            if (!String.IsNullOrEmpty(_searchBeforeTag))
            {
                _searchBeforeTag = string.Format(_templateTag, _searchBeforeTag);

                if (aTagIndex >= _content.IndexOf(_searchBeforeTag)) return tag_templates;
            }

            Template.Position tagPosition = new Position(ref _content, ref _aTag, ref _bTag);

            //bTagIndex = content.IndexOf(bTag, aTagIndex + 1);
            //
            //int tmpATagIndex = aTagIndex;
            //
            bTagIndex = tagPosition.GetEndPoint(aTagIndex);

            //string tmpBlockStr = "";

            while (aTagIndex != -1 && bTagIndex != -1 && (bTagIndex + bTagLength) <= _content.Length)
            {
                if (_foreachATagPredicate != null)
                {
                    if (!_foreachATagPredicate(aTagIndex, ref _content))
                    {

                        aTagIndex = _content.IndexOf(_aTag, bTagIndex + 1);

                        bTagIndex = tagPosition.GetEndPoint(ref _content, aTagIndex);

                        continue;
                    }
                }

                blockIndex++;

                BlockInfo blockInfo = new BlockInfo();

                blockInfo.Value = _content.Substring(aTagIndex + aTagLength, bTagIndex - (aTagIndex + aTagLength));

                if (!hasReplaceHolderTemplateTag)
                {
                    /*
                        [o1]<!-- Template_TABLE -->[i1]
                        {Tag}
                        [i2]<!-- Template_TABLE -->[o2]
                     */

                    // [i1] <-> [i2] <==> InnerAIndex <-> InnerBIndex
                    //
                    blockInfo.InnerAIndex = aTagIndex + aTagLength;
                    blockInfo.InnerBIndex = bTagIndex - 1;

                    // [o1](<...) <-> [o2](...>) <==> OuterAIndex ( < index ) <-> OuterBIndex ( > index )
                    //
                    blockInfo.OuterAIndex = aTagIndex;
                    blockInfo.OuterBIndex = bTagIndex + bTagLength - 1;
                }

                if (hasReplaceHolderTemplateTag)
                {
                    replaceHolderTemplateTagTmp = string.Format(_replaceHolderTemplateTag, blockIndex);

                    _content = _content.Substring(0, aTagIndex)

                        +
                        replaceHolderTemplateTagTmp
                        +

                        _content.Substring(bTagIndex + bTagLength);

                    bTagIndex = _content.IndexOf(replaceHolderTemplateTagTmp, aTagIndex);

                    if (bTagIndex == -1)
                    {
                        throw new ArgumentException("Block : ReplaceHolderTemplateTag Not Found");
                    }

                    bTagIndex += replaceHolderTemplateTagTmp.Length - 1;

                    blockInfo.ReplaceHolderTemplateTag = replaceHolderTemplateTagTmp;
                }

                //if (_fillTagInfos)
                //{
                //    // # DATA Item & Template.TagInfo #
                //    //
                //    blockInfo.FillTagInfos();
                //}

                tag_templates.Add(blockInfo);

                if (_targetBlockIndex == blockIndex) break;

                aTagIndex = _content.IndexOf(_aTag, bTagIndex + 1);
                //
                //bTagIndex = content.IndexOf(bTag, aTagIndex + 1);
                //
                //tmpATagIndex = aTagIndex + 1;
                //
                //tmpATagIndex = aTagIndex;
                //
                bTagIndex = tagPosition.GetEndPoint(ref _content, aTagIndex);
            }

            return tag_templates;
        }

        public static bool operator +(Template _a, Template _b)
        {
            if (_a.isEmpty && _b.isEmpty) return false;

            Template c_a;
            Template c_b;

            if (_a.isIndexTemplate && !_b.isIndexTemplate)
            {
                c_a = _a;
                c_b = _b;
            }
            else if (_b.isIndexTemplate && !_a.isIndexTemplate)
            {
                c_a = _b;
                c_b = _a;
            }
            else
            {
                if (_a.content.IndexOf("{" + _b.tag + "}") != -1)
                {
                    c_a = _a;
                    c_b = _b;
                }
                else if (_b.content.IndexOf("{" + _a.tag + "}") != -1)
                {
                    c_a = _b;
                    c_b = _a;
                }
                else
                {
                    return false;
                }
            }

            if (String.IsNullOrEmpty(c_b.ToString()))
            {
                // c_a share -> c_b
                //
                if (!Template.TransFerTo(c_a, c_b))
                {
                    return false;
                }
            }

            c_a.content = c_a.content.Replace("{" + c_b.tag + "}", c_b.content);

            return true;
        }

        public static bool TransFerTo(Template _a, Template _b)
        {
            if (Template.Root.hasPrivateShareTemplate)
            {
                if (File.Exists(string.Format(@"{0}\{1}\{2}", _a.rootPath, Template.PrivateShareDocumentFolder, _b.tag + ".html")))
                {
                    _b.InitPathInfos(_b.tag + ".html", string.Format(@"{0}\{1}\", _a.rootPath, Template.PrivateShareDocumentFolder));

                    return _b.ScanDocument();
                }
            }

            if (!String.IsNullOrEmpty(Template.Root.shareDocumentPath.FullName))
            {
                if (File.Exists(string.Format(@"{0}\{1}", Template.Root.shareDocumentPath.FullName, _b.tag + ".html")))
                {
                    _b.InitPathInfos(_b.tag + ".html", Template.Root.shareDocumentPath.FullName);

                    return _b.ScanDocument();
                }
            }

            return false;
        }

        public void Content(ref string _content)
        {
            this.content = _content;
        }

        public string ToString()
        {
            return this.content;
        }

        public static int GetEndPoint(ref string content, ref string aTag, ref string bTag, ref int tmpATagIndex)
        {
            return 0;
        }

        public class Position
        {
            private string str = "";
            private int str_length = 0;
            private int tmp_index = 0;
            private int tmp_index2 = 0;
            private char aTag_c;
            private string aTag = "";
            private int aTagIndex = 0;
            private int aTagLength = 0;
            private int aTagCount = 0;
            private string bTag = "";
            private char bTag_c;
            private int bTagIndex = 0;
            private int bTagLength = 0;
            private int bTagCount = 0;
            private int loopIndex = -1;

            public Position()
            { }

            public Position(ref string _str, string _aTag, string _bTag, int _startIndex = -1)
            {
                this.NewPosition(_str, _aTag, _bTag, _startIndex);
            }

            public Position(ref string _str, ref string _aTag, ref string _bTag, int _startIndex = -1)
            {
                this.NewPosition(_str, _aTag, _bTag, _startIndex);
            }

            public Position(string _str, string _aTag, string _bTag, int _startIndex = -1)
            {
                this.NewPosition(_str, _aTag, _bTag, _startIndex);
            }

            public Position(string _str, char _aTag, char _bTag, int _startIndex = -1)
            {
                this.NewPosition(_str, _aTag, _bTag, _startIndex);
            }

            public void NewPosition(char _aTag, char _bTag, int _startIndex = -1)
            {
                this.aTag_c = _aTag;
                this.bTag_c = _bTag;
                this.aTagLength = 1;
                this.bTagLength = 1;

                this.NewPosition(_startIndex);
            }

            public void NewPosition(string _aTag, string _bTag, int _startIndex = -1)
            {
                this.aTag = _aTag;
                this.bTag = _bTag;
                this.aTagLength = _aTag.Length;
                this.bTagLength = _bTag.Length;

                this.NewPosition(_startIndex);
            }

            public void NewPosition(int _startIndex = -1)
            {
                this.tmp_index = 0;
                this.tmp_index2 = 0;
                //this.aTag = _aTag;
                this.aTagIndex = 0;
                //this.aTagLength = _aTag.Length;
                this.aTagCount = 0;
                //this.bTag = _bTag;
                this.bTagIndex = 0;
                //this.bTagLength = _bTag.Length;
                this.bTagCount = 0;
                this.loopIndex = _startIndex;
            }

            public void NewPosition(string _str, string _aTag, string _bTag, int _startIndex = -1)
            {
                this.str = _str;
                this.str_length = _str.Length;

                this.NewPosition(_aTag, _bTag, _startIndex);
            }

            public void NewPosition(string _str, char _aTag, char _bTag, int _startIndex = -1)
            {
                this.str = _str;
                this.str_length = _str.Length;

                this.NewPosition(_aTag, _bTag, _startIndex);
            }

            public int GetEndPoint(int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                this.str_length = this.str.Length;

                return Position.EndPoint(ref this.str, ref this.str_length, ref this.tmp_index, ref this.tmp_index2, ref this.aTag, ref this.aTagCount, ref this.bTag, ref this.bTagCount, ref this.loopIndex);
            }

            public int GetEndPoint(ref string _str, int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                this.str = _str;

                this.str_length = this.str.Length;

                return Position.EndPoint(ref this.str, ref this.str_length, ref this.tmp_index, ref this.tmp_index2, ref this.aTag, ref this.aTagCount, ref this.bTag, ref this.bTagCount, ref this.loopIndex);
            }

            public string GetLineTag(int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                return Position.LineTag(ref this.str, ref this.str_length, ref this.aTag, ref this.tmp_index, ref this.tmp_index2, ref this.loopIndex);
            }

            public string GetLineTag(ref string _aTag, int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                return Position.LineTag(ref this.str, ref this.str_length, ref this.aTag, ref this.tmp_index, ref this.tmp_index2, ref this.loopIndex);
            }

            public static string LineTag(ref string _str, ref int _str_length, ref string _tag, ref int _tmpIndex, ref int _tmpIndex2, ref int _loopIndex)
            {
                string tmp = "";

                _tmpIndex = _str.IndexOf(_tag, _loopIndex);

                if (_tmpIndex == -1) return tmp;

                _str_length = "\r\n".Length;

                _tmpIndex2 = _str.LastIndexOf("\r\n", _tmpIndex);

                tmp = _str.Substring(_str_length + _tmpIndex2, _tmpIndex - (_str_length + _tmpIndex2));

                return "\r\n" + tmp + _tag;
            }

            public static int EndPoint(ref string _str, ref int _str_length, ref int _tmpIndex, ref int _tmpIndex2, ref string _aTag, ref int _aTagCount, ref string _bTag, ref int _bTagCount, ref int _loopIndex)
            {
                // ====================

                //if (_aTag == _bTag)
                //{
                //    string tmp = Position.GetFullLineTag(ref _str, ref _str_length, ref _aTag, ref _tmpIndex, ref _tmpIndex2, ref _loopIndex);

                //    if (!String.IsNullOrEmpty(tmp))
                //    {
                //        _aTag = tmp;
                //        _bTag = tmp;
                //    }

                //    _str_length = _str.Length;
                //    _tmpIndex = 0;
                //    _tmpIndex2 = 0;
                //}

                // ====================

                if (_loopIndex == -1) return -1;

                _tmpIndex = _str.IndexOf(_aTag, _loopIndex);

                if (_tmpIndex == -1) return -1;

                _aTagCount++;
                _loopIndex = _tmpIndex;

                //string str = "{ {} {{{}}} {}}";
                //              012345678901234

                while ((_loopIndex + 1) < _str_length)
                {
                    if (_aTagCount == _bTagCount && _aTagCount >= 1) break;

                    _tmpIndex = _str.IndexOf(_aTag, _loopIndex + 1);
                    _tmpIndex2 = _str.IndexOf(_bTag, _loopIndex + 1);

                    if (_tmpIndex == -1 && _tmpIndex2 == -1)
                    {
                        break;
                    }

                    // =====================

                    if (_tmpIndex != -1 && _tmpIndex == _tmpIndex2)
                    {
                        _loopIndex = _tmpIndex;

                        break;
                    }

                    // =====================

                    if ((_tmpIndex != -1 && _tmpIndex < _tmpIndex2)
                        ||
                        _tmpIndex2 == -1)
                    {
                        _aTagCount++;

                        _loopIndex = _tmpIndex;

                        continue;
                    }

                    if ((_tmpIndex2 != -1 && _tmpIndex2 < _tmpIndex)
                        ||
                        _tmpIndex == -1)
                    {
                        _bTagCount++;

                        _loopIndex = _tmpIndex2;

                        continue;
                    }
                }

                return _loopIndex;
            }

            public List<string> GetLongTags(int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                return Position.LongTags(ref this.str, ref this.str_length, ref this.aTag, ref this.aTagIndex, ref this.aTagLength, ref this.bTag, ref this.bTagIndex, ref this.bTagLength, ref this.loopIndex);
            }

            public static List<string> LongTags(ref string _content, ref int _contentIndex, ref string _aTag, ref int _aTagIndex, ref int _aTagLength, ref string _bTag, ref int _bTagIndex, ref int _bTagLength, ref int _loopIndex)
            {
                List<string> tmpList = new List<string>();

                _contentIndex = _content.Length - 1;

                string tmpStr = "";

                _aTagIndex = _content.IndexOf(_aTag, _loopIndex);

                if (_aTagIndex == -1) return tmpList;

                _bTagIndex = _content.IndexOf(_bTag, _aTagIndex + 1);

                while (_aTagIndex != -1 && _aTagIndex < _bTagIndex)
                {
                    tmpStr = _content.Substring(_aTagIndex + _aTagLength, _bTagIndex - (_aTagIndex + _aTagLength));

                    tmpList.Add(tmpStr);

                    _aTagIndex = _content.IndexOf(_aTag, _bTagIndex + 1);

                    if (_aTagIndex == -1) break;

                    _bTagIndex = _content.IndexOf(_bTag, _aTagIndex + 1);
                }

                return tmpList;
            }

            public List<string> GetTags(int _startIndex = 0)
            {
                this.loopIndex = _startIndex;

                return Position.Tags(ref this.str, ref this.str_length, ref this.aTag_c, ref this.aTagIndex, ref this.bTag_c, ref this.bTagIndex, ref this.loopIndex);
            }

            public static List<string> Tags(ref string _content, ref int _contentIndex, ref char _aTag, ref int _aTagIndex, ref char _bTag, ref int _bTagIndex, ref int _loopIndex)
            {
                List<string> tmpList = new List<string>();

                _contentIndex = _content.Length - 1;

                string tmpStr = "";
                char charStr;

                _loopIndex = _content.IndexOf(_aTag, _loopIndex);

                if (_loopIndex == -1) return tmpList;

                _loopIndex++;
                _aTagIndex = _loopIndex;

                while (_loopIndex <= _contentIndex)
                {
                    charStr = Convert.ToChar(_content.Substring(_loopIndex, 1));

                    if (charStr == _bTag)
                    {
                        _bTagIndex = _loopIndex;
                        tmpStr = _content.Substring(_aTagIndex, _bTagIndex - _aTagIndex);

                        tmpList.Add(tmpStr);

                        _loopIndex++;

                        continue;
                    }

                    // next char
                    //
                    if (Template.tag_chars.IndexOf(charStr) == -1)
                    {
                        _loopIndex = _content.IndexOf(_aTag, _loopIndex + 1);

                        if (_loopIndex == -1) break;

                        _loopIndex++;
                        _aTagIndex = _loopIndex;

                        continue;
                    }

                    _loopIndex++;
                }

                return tmpList;
            }

        }

        public class BlockInfo
        {
            public BlockInfo()
            {

            }

            public BlockInfo(BlockInfo _blockInfo, Template _template)
            {
                if (_blockInfo != null)
                {
                    this.Value = _blockInfo.Value;
                    this.InnerAIndex = _blockInfo.InnerAIndex;
                    this.InnerBIndex = _blockInfo.InnerBIndex;
                    this.OuterAIndex = _blockInfo.OuterAIndex;
                    this.OuterBIndex = _blockInfo.OuterBIndex;

                    this.Tags = _blockInfo.Tags;

                    //this.TagInfosReplaceHolderTemplateTag = _blockInfo.TagInfosReplaceHolderTemplateTag;

                    this.ReplaceHolderTemplateTag = _blockInfo.ReplaceHolderTemplateTag;
                }

                this.Template = _template;
            }

            public BlockInfo(BlockInfo _blockInfo)
            {
                if (_blockInfo != null)
                {
                    this.Value = _blockInfo.Value;
                    this.InnerAIndex = _blockInfo.InnerAIndex;
                    this.InnerBIndex = _blockInfo.InnerBIndex;
                    this.OuterAIndex = _blockInfo.OuterAIndex;
                    this.OuterBIndex = _blockInfo.OuterBIndex;

                    this.Tags = _blockInfo.Tags;

                    //this.TagInfosReplaceHolderTemplateTag = _blockInfo.TagInfosReplaceHolderTemplateTag;

                    this.ReplaceHolderTemplateTag = _blockInfo.ReplaceHolderTemplateTag;

                    this.Template = _blockInfo.Template;
                }
            }

            public string Value { get; set; }

            public Dictionary<string, List<TagInfo>> Tags { get; set; }

            //public string TagInfosReplaceHolderTemplateTag { get; set; }

            public int InnerAIndex { get; set; }
            public int InnerBIndex { get; set; }

            public int OuterAIndex { get; set; }
            public int OuterBIndex { get; set; }

            public string ReplaceHolderTemplateTag { get; set; }

            public Template Template { get; set; }
        }

        public class TagInfo
        {
            // [] { TagType.Fixed TagType.Combo }
            public List<TagType> TagTypes { get; set; }

            // "Fixed $$ButtonAdd"
            public string Expression { get; set; }

            public Dictionary<TagType, string> ExpressionItems { get; set; }

            public List<TagInfo.Rnd> TemplateRnds { get; set; }

            public A4.DATA.Model.Rnd<IItemExtend> GroupItems { get; set; }

            // # DATA Item & Template.TagInfo #
            //
            // ? ! == List<RowItems> => List<Item> => Item => Rnd (GroupItems):
            //tagInfo.BoundColumns = null;

            // html
            public string TemplateHolder { get; set; }

            public string ReplaceHolderTemplateTag { get; set; }

            public class Rnd
            {
                // "$$ButtonAdd.Rnd"
                public string Name { get; set; }
                // "QWWFJE24DWIFJ0"
                public string Value { get; set; }
                // html
                public Template.BlockInfo TemplateHolder { get; set; }
            }
        }

        public class Table : Block<TR>
        {
            public Table(BlockInfo _blockTemplateHolder, Template _template)
            {
                this.TemplateHolder = new BlockInfo(_blockTemplateHolder, _template);

                this.GTRs = new Group<TR>();

                this.TLogs = new System.Collections.Generic.List<TLog>();
            }

            public Table(BlockInfo _blockTemplateHolder)
            {
                this.TemplateHolder = new BlockInfo(_blockTemplateHolder, _blockTemplateHolder.Template);

                this.GTRs = new Group<TR>();

                this.TLogs = new System.Collections.Generic.List<TLog>();
            }

            public List<TLog> TLogs { get; set; }

            public class TLog
            {
                public TLog(int _trGroupIndex, int _trIndex)
                {
                    this.TrGroupIndex = _trGroupIndex;
                    this.TrIndex = _trIndex;
                }

                public int TrGroupIndex { get; set; }
                public int TrIndex { get; set; }
            }
        }

        public class Block<T> : IBlock where T : IBlock
        {
            public BlockInfo TemplateHolder { get; set; }
            public string Content { get; set; }
            public Group<T> GTRs { get; set; }

            public class Group<T> where T : IBlock
            {
                public Group()
                {
                    this.tGroup = new System.Collections.Generic.Dictionary<int, TGroup<T>>();
                }

                public Group(BlockInfo _blockTemplate)
                {
                    this.tGroup = new System.Collections.Generic.Dictionary<int, TGroup<T>>();

                    this.tGroup.Add(0, new TGroup<T>(_blockTemplate));
                }

                public TGroup<T> this[int index]
                {
                    get { return this.tGroup.ContainsKey(index) ? this.tGroup[index] : null; }

                    set
                    {

                        if (this.tGroup.ContainsKey(index))
                        {
                            this.tGroup[index] = value;
                        }
                        else
                        {
                            this.tGroup.Add(index, value);
                        }
                    }
                }

                public int Count { get { return this.tGroup.Keys.Count; } }

                private Dictionary<int, TGroup<T>> tGroup { get; set; }
            }

            public class TGroup<T> : Template.Block
            {
                public TGroup()
                {
                    this.TRs = new System.Collections.Generic.List<T>();
                }

                public TGroup(BlockInfo _blockTemplate)
                {
                    this.TemplateHolder = new BlockInfo(_blockTemplate);

                    this.TRs = new System.Collections.Generic.List<T>();

                    this.InnerTable = null;

                    this.InnerTables = null;
                }

                public TGroup(BlockInfo _blockTemplate, BlockInfo _innerTableBlockTemplate)
                {
                    this.TemplateHolder = new BlockInfo(_blockTemplate);

                    this.TRs = null;

                    this.InnerTable = new Table(_innerTableBlockTemplate);

                    this.InnerTables = null;
                }

                public TGroup(BlockInfo _blockTemplate, List<BlockInfo> _innerTablesBlockTemplate)
                {
                    this.TemplateHolder = new BlockInfo(_blockTemplate);

                    this.TRs = null;

                    this.InnerTable = null;

                    this.InnerTables = new System.Collections.Generic.List<Table>();

                    foreach (var item in _innerTablesBlockTemplate)
                    {
                        this.InnerTables.Add(new Table(item));
                    }
                }

                public List<T> TRs { get; set; }

                public Table InnerTable { get; set; }

                public List<Table> InnerTables { get; set; }
            }
        }

        //public class Table
        //{
        //    public BlockInfo TemplateHolder { get; set; }
        //    public string Content { get; set; }
        //    public List<TR> TRs { get; set; }
        //}

        public class TR : Block
        {
            public TR()
            {

            }

            public TR(TR _tr)
                : base(_tr.Pairs)
            {
                this.TemplateHolder = new BlockInfo(_tr.TemplateHolder);

                if (_tr.Container != null)
                {
                    this.Container = new BlockInfo(_tr.Container);
                }

                this.TDs = new List<TD>();

                _tr.TDs.ForEach(m => { this.TDs.Add(new TD(m.Pairs, m.TemplateHolder, m.Container)); return; });
            }

            public TR(BlockInfo _templateHolder, BlockInfo _container)
            {
                this.TemplateHolder = new BlockInfo(_templateHolder);

                if (_container != null)
                {
                    this.Container = new BlockInfo(_container);
                }

                this.TDs = new List<TD>();
            }

            //public BlockInfo TemplateHolder { get; set; }
            //public string Content { get; set; }

            public BlockInfo Container { get; set; }

            public List<TD> TDs { get; set; }

            public static TR operator +(TR _tr, List<Page.RenderModel> _renderModels)
            {
                TD td = _tr.TDs.FirstOrDefault<TD>(m => !m.IsRendered);

                if (td == null)
                {
                    _tr.TDs.Add(new TD(_tr.TDs[0]));

                    td = _tr.TDs[_tr.TDs.Count - 1];
                }

                td.Content = td.TemplateHolder.Value;

                _renderModels.TemplateRender(td);

                if (td.Container != null)
                {
                    td.Container.Combine(td.TemplateHolder, td);

                    _renderModels.TemplateRender(td.Container);
                }

                td.IsRendered = true;

                return _tr;
            }

            public static TR operator +(TR _tr, A4.DATA.Model.Item _dataTd)
            {
                TD td = _tr.TDs.FirstOrDefault<TD>(m => !m.IsRendered);

                if (td == null)
                {
                    _tr.TDs.Add(new TD(_tr.TDs[0]));

                    td = _tr.TDs[_tr.TDs.Count - 1];
                }

                td.Content = td.TemplateHolder.Value;

                // ===

                List<Page.RenderModel> renderModels = _dataTd.ToRenderModels<A4.DATA.Model.Item>();

                // ===

                //_dataTd.Value = _dataTd.Value ?? "";
                //
                // #1 data & extend params => render => item (tagContent)
                //
                // ...
                //
                //td.Content = td.Content.Replace("{" + _dataTd.ColumnName + "}", _dataTd.Value.ToString());

                // =======================

                Dictionary<string, List<Template.TagInfo>> tags = null;

                List<Template.TagInfo> innerTags = null;

                string tagContent = "";
                string tagsContent = "";

                if (!td.TemplateHolder.Tags.IsEmpty())
                {
                    tags = td.TemplateHolder.Tags.Filter(_dataTd, (IItemExtend)_dataTd);

                    foreach (string replaceHolderTag in tags.Keys)
                    {
                        innerTags = tags[replaceHolderTag];

                        foreach (var item in innerTags)
                        {
                            // ########

                            // get => List<Page.RenderModel> renderModelsExtend => by List<Template.TagInfo> (tags) for one item

                            // #2 data & extend params => render => item (tagContent)

                            tagContent = item.TemplateHolder;

                            renderModels.TemplateRender(ref tagContent);

                            tagsContent += tagContent;
                        }

                        //if (!String.IsNullOrEmpty(tagsContent))
                        //{
                        td.Content = td.Content.Replace(replaceHolderTag, tagsContent);
                        //}
                    }
                }

                // =======================

                renderModels.TemplateRender(td);

                // =======================

                if (td.Container != null)
                {
                    // #3 data & extend params => render => item (tagContent)
                    //
                    // ...
                    //
                    //td.Container.Value = td.Container.Value.Replace("{" + _dataTd.ColumnName + "}", _dataTd.Value.ToString());

                    //td.Container.Combine(td.TemplateHolder, td);

                    renderModels.TemplateRender(td.Container);
                }

                td.IsRendered = true;

                return _tr;
            }

            public static TR operator +(TR _tr, System.Data.DataRow _dataTd)
            {
                TD td = _tr.TDs.FirstOrDefault<TD>(m => !m.IsRendered);

                if (td == null)
                {
                    _tr.TDs.Add(new TD(_tr.TDs[0]));

                    td = _tr.TDs[_tr.TDs.Count - 1];
                }

                td.Content = td.TemplateHolder.Value;

                for (int i = 0, iMax = _dataTd.Table.Columns.Count; i < iMax; i++)
                {
                    System.Data.DataColumn dataColumn = _dataTd.Table.Columns[i];

                    string columnName = dataColumn.ColumnName;
                    object columnValue = _dataTd[dataColumn];

                    columnValue = columnValue ?? "";

                    td.Content = td.Content.Replace("{" + columnName + "}", columnValue.ToString());

                    if (td.Container != null)
                    {

                        td.Container.Value = td.Container.Value.Replace("{" + columnName + "}", columnValue.ToString());
                    }
                }

                if (td.Container != null)
                {
                    td.Container.Combine(td.TemplateHolder, td);
                }

                td.IsRendered = true;

                return _tr;
            }

            public bool IsRendered { get; set; }
        }

        public class TD : Block
        {
            public TD()
            {

            }

            public TD(TD _td)
                : base(_td.Pairs)
            {
                this.TemplateHolder = new BlockInfo(_td.TemplateHolder);

                if (_td.Container != null)
                {
                    this.Container = new BlockInfo(_td.Container);
                }

                this.IsRendered = false;
            }

            public TD(List<string> _pairs, BlockInfo _blockInfoTemplate, BlockInfo _blockInfoContainer = null)
                : base(_pairs)
            {
                this.TemplateHolder = new BlockInfo(_blockInfoTemplate);

                if (_blockInfoContainer != null)
                {
                    this.Container = new BlockInfo(_blockInfoContainer);
                }

                this.IsRendered = false;
            }

            //public BlockInfo TemplateHolder { get; set; }
            //public string Content { get; set; }

            public BlockInfo Container { get; set; }

            public bool IsRendered { get; set; }
        }

        public class Block : IBlock
        {
            public Block()
            { }

            public Block(List<string> _pairs)
            {
                if (_pairs != null)
                {
                    this.Pairs = new List<string>();

                    _pairs.ForEach(m =>
                    {
                        this.Pairs.Add(m);
                    });
                }
            }

            public BlockInfo TemplateHolder { get; set; }
            public string Content { get; set; }
            public Page.IDATA DATA { get; set; }
            public List<string> Pairs { get; set; }
        }

        public interface IBlock
        {
            string Content { get; set; }

            //bool IsRender { get; set; }
        }

        public enum TagType
        {
            None = 0,
            Default,
            Fixed,
            Combo,
            ColumnValue
        }
    }
}
