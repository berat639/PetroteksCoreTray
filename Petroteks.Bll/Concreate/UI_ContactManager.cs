using Petroteks.Bll.Abstract;
using Petroteks.Dal.Abstract;
using Petroteks.Entities.ComplexTypes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static Petroteks.Bll.Helpers.LanguageContext;

namespace Petroteks.Bll.Concreate
{
    public class UI_ContactManager : EntityManager<UI_Contact>, IUI_ContactService
    {
        public UI_ContactManager(IUI_ContactDal repostory) : base(repostory)
        {
        }
        public override UI_Contact Get(Expression<Func<UI_Contact, bool>> filter, int LangId, params string[] navigations)
        {
            filter = LanguageControl(filter, LangId);
            return base.Get(filter, LangId, navigations);
        }
        public override ICollection<UI_Contact> GetMany(Expression<Func<UI_Contact, bool>> filter, int LangId, params string[] navigations)
        {
            filter = LanguageControl(filter, LangId);
            return base.GetMany(filter, LangId, navigations);
        }

        public ICollection<UI_Contact> LanguageAndWebsiteFilteredData(int websiteId, int languageId)
        {
            return base.GetMany(x => x.IsActive && x.IsActive && x.WebSiteid == websiteId && x.Languageid == languageId);
        }
    }
}
