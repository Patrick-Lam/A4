using System;

namespace A4.Tools
{
    public class Pager : IPager
    {
        private int m_maxPage = 0;

        // ===

        public int MaxRecords = 0;
        public int MaxPage { get { return this.m_maxPage + this.lastReset_CurrentPage; } set { this.m_maxPage = value; } }
        public int PageSize = 0;
        public int CurrentPage = 0;
        public int LastPageSurplusRecords = 0;

        private bool hasSurplusRecords = false;
        protected int nextPage = 0;

        public PagerInfo PagerInfo;

        // === 

        private int lastReset_DefaultPageInfoEndIndex = 0;
        private int lastReset_CurrentPage = 0;
        private int lastReset_PassRecords = 0;

        private bool isResetted = false;

        // ===

        public Pager()
        { }

        public Pager(int _maxRecords, int _pageSize, int _currentPage = 1)
        {
            this.MaxRecords = _maxRecords;
            this.PageSize = _pageSize;
            this.CurrentPage = _currentPage;

            this.InitPagerInfo();
        }

        public void Reset(int _pageSize)
        {
            if (this.PageSize == _pageSize) return;

            this.lastReset_DefaultPageInfoEndIndex = this.PagerInfo.DefaultPageInfo.endIndex;
            this.lastReset_CurrentPage = this.CurrentPage;
            this.lastReset_PassRecords = this.PagerInfo.DefaultPageInfo.endIndex;

            this.PageSize = _pageSize;

            this.isResetted = true;

            // ===

            //this.PagerInfo = new PagerInfo();

            //this.PagerInfo.DefaultPageInfo = new PageIndex();
            //this.PagerInfo.LastPageSurplusRecordsIndex = new PageIndex();

            this.InitPagerInfo();
        }

        public virtual bool Next(int _nextPageSize = -1)
        {
            if (_nextPageSize != -1)
            {
                this.Reset(_nextPageSize);
            }

            if (this.nextPage <= 0) this.nextPage = 1;

            if (this.nextPage <= this.MaxPage)
            {
                this.Page(this.nextPage);

                this.nextPage++;

                return true;
            }
            else
            {
                this.nextPage = -1;

                return false;
            }
        }

        public PagerInfo Page(int _currentPage)
        {
            if (this.isResetted)
            {
                if (_currentPage <= this.lastReset_CurrentPage)
                {
                    throw new ArgumentException(string.Format("Pager Resetted PageSize At Page [{0}]", this.lastReset_CurrentPage));
                }
            }

            this.CurrentPage = _currentPage;

            if (this.CurrentPage > this.MaxPage) this.CurrentPage = this.MaxPage;
            if (this.CurrentPage < 1) this.CurrentPage = 1;

            this.PagerInfo.IsFirstPage = false;
            this.PagerInfo.IsLastPage = false;

            if (this.CurrentPage == 1) this.PagerInfo.IsFirstPage = true;
            if (this.CurrentPage == this.MaxPage) this.PagerInfo.IsLastPage = true;

            int beginIndex = 0;
            int endIndex = 0;

            //beginIndex = (this.CurrentPage - 1) * this.PageSize + 1;
            //
            beginIndex += this.lastReset_DefaultPageInfoEndIndex;
            beginIndex += (this.CurrentPage - this.lastReset_CurrentPage - 1) * this.PageSize + 1;
            //
            endIndex = (beginIndex + this.PageSize - 1);

            if (this.PagerInfo.IsLastPage)
            {
                if (this.hasSurplusRecords)
                {
                    endIndex = this.MaxRecords;
                }
            }

            this.PagerInfo.DefaultPageInfo.beginIndex = beginIndex;
            this.PagerInfo.DefaultPageInfo.endIndex = endIndex;

            if (this.PagerInfo.IsLastPage)
            {
                if (this.LastPageSurplusRecords >= 1)
                {
                    beginIndex = endIndex + 1;

                    //endIndex = this.MaxPage * this.PageSize;
                    //
                    endIndex = this.lastReset_PassRecords + (this.m_maxPage * this.PageSize);
                }
                else
                {
                    beginIndex = 0;
                    endIndex = 0;
                }

                this.PagerInfo.LastPageSurplusRecordsIndex.beginIndex = beginIndex;
                this.PagerInfo.LastPageSurplusRecordsIndex.endIndex = endIndex;
            }

            #region #

            //if (this.PagerInfo.IsFirstPage)
            //{
            //    this.PagerInfo.DefaultPageInfo.beginIndex = (this.CurrentPage - 1) * this.PageSize + 1;

            //    if (this.PagerInfo.IsLastPage)
            //    {
            //        this.PagerInfo.DefaultPageInfo.endIndex = this.PagerInfo.DefaultPageInfo.beginIndex + (this.MaxRecords - (this.PageSize * (this.MaxPage - 1)) - this.LastPageSurplusRecords) - 1;
            //    }
            //    else
            //    {
            //        this.PagerInfo.DefaultPageInfo.endIndex = this.PagerInfo.DefaultPageInfo.beginIndex + this.PageSize - 1;
            //    }
            //}

            //if (this.PagerInfo.IsLastPage)
            //{
            //    this.PagerInfo.DefaultPageInfo.beginIndex = (this.CurrentPage - 1) * this.PageSize + 1;

            //    if (this.LastPageSurplusRecords >= 1)
            //    {
            //        this.PagerInfo.DefaultPageInfo.endIndex = this.PagerInfo.DefaultPageInfo.beginIndex + (this.MaxRecords - (this.PageSize * (this.MaxPage - 1)) - this.LastPageSurplusRecords) - 1;
            //    }
            //    else
            //    {
            //        this.PagerInfo.DefaultPageInfo.endIndex = this.PagerInfo.DefaultPageInfo.beginIndex + this.PageSize - 1;                
            //    }
            //}

            //if (this.PagerInfo.IsLastPage && this.LastPageSurplusRecords >= 1)
            //{
            //    this.PagerInfo.LastPageSurplusRecordsIndex.beginIndex = this.PagerInfo.DefaultPageInfo.endIndex + 1;
            //    this.PagerInfo.LastPageSurplusRecordsIndex.endIndex = this.MaxRecords;
            //}
            //else
            //{
            //    this.PagerInfo.LastPageSurplusRecordsIndex.beginIndex = 0;
            //    this.PagerInfo.LastPageSurplusRecordsIndex.endIndex = 0;
            //}

            #endregion

            return this.PagerInfo;
        }

        protected void InitPagerInfo()
        {
            this.PagerInfo = new PagerInfo();

            this.PagerInfo.DefaultPageInfo = new PageIndex();
            this.PagerInfo.LastPageSurplusRecordsIndex = new PageIndex();

            // ===

            //if (this.MaxRecords % this.PageSize == 0)
            //{
            //    this.MaxPage = this.MaxRecords / this.PageSize;
            //    this.hasSurplusRecords = false;
            //}
            //else
            //{
            //    this.MaxPage = Convert.ToInt32((this.MaxRecords / this.PageSize)) + 1;
            //    this.hasSurplusRecords = true;
            //}

            //this.LastPageSurplusRecords = this.MaxPage * this.PageSize - this.MaxRecords;

            // ==> this.Reset(1);

            int currentMaxRecordsForResetPage = (this.MaxRecords - this.lastReset_PassRecords);

            if (currentMaxRecordsForResetPage % this.PageSize == 0)
            {
                //this.MaxPage = this.MaxRecords / this.PageSize;
                //
                this.MaxPage = currentMaxRecordsForResetPage / this.PageSize;

                this.hasSurplusRecords = false;
            }
            else
            {
                //this.MaxPage = Convert.ToInt32((this.MaxRecords / this.PageSize)) + 1;
                //
                this.MaxPage = Convert.ToInt32((currentMaxRecordsForResetPage / this.PageSize)) + 1;

                this.hasSurplusRecords = true;
            }

            this.LastPageSurplusRecords = this.m_maxPage * this.PageSize - (this.MaxRecords - this.lastReset_PassRecords);
        }
    }

    public interface IPager
    {
        bool Next(int _nextPageSize = -1);
    }

    public class PagerInfo
    {
        public bool IsFirstPage { get; set; }
        public bool IsLastPage { get; set; }
        public PageIndex DefaultPageInfo { get; set; }
        public PageIndex LastPageSurplusRecordsIndex { get; set; }
    }

    public class PageIndex
    {
        public PageIndex()
        {
            this.beginIndex = 0;
            this.endIndex = 0;
        }

        public int beginIndex { get; set; }
        public int endIndex { get; set; }
    }
}
