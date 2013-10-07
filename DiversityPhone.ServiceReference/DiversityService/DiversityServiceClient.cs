
using DiversityPhone.DiversityService;
using DiversityPhone.Interface;
using DiversityPhone.MultimediaService;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using Client = DiversityPhone.Model;

namespace DiversityPhone.Services
{
    public partial class DiversityServiceClient : IDiversityServiceClient
    {
        DiversityService.DiversityServiceClient _svc = new DiversityService.DiversityServiceClient();
        MapService.PhoneMediaServiceClient _maps = new MapService.PhoneMediaServiceClient();
        MultimediaService.MediaService4Client _multimedia = new MultimediaService.MediaService4Client();

        //MISC
        IObservable<EventPattern<GetUserInfoCompletedEventArgs>> GetUserInfoCompleted;
        IObservable<EventPattern<GetRepositoriesCompletedEventArgs>> GetRepositoriesCompleted;
        IObservable<EventPattern<GetPropertiesForUserCompletedEventArgs>> GetPropertiesForUserCompleted;
        IObservable<EventPattern<GetProjectsForUserCompletedEventArgs>> GetProjectsForUserCompleted;


        //VOCABULARY
        IObservable<EventPattern<GetStandardVocabularyCompletedEventArgs>> GetStandardVocabularyCompleted;
        IObservable<EventPattern<GetTaxonListsForUserCompletedEventArgs>> GetTaxonListsForUser;
        IObservable<EventPattern<DownloadTaxonListCompletedEventArgs>> DownloadTaxonList;
        IObservable<EventPattern<GetQualificationsCompletedEventArgs>> GetQualificationsCompleted;
        IObservable<EventPattern<GetAnalysesForProjectCompletedEventArgs>> GetAnalysesForProjectCompleted;
        IObservable<EventPattern<GetAnalysisResultsForProjectCompletedEventArgs>> GetAnalysisResultsForProjectCompleted;
        IObservable<EventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>> GetAnalysisTaxonomicGroupsForProjectCompleted;

        // UPLOAD
        IObservable<EventPattern<AsyncCompletedEventArgs>> InsertMMOCompleted;
        IObservable<EventPattern<InsertEventSeriesCompletedEventArgs>> InsertESCompleted;
        IObservable<EventPattern<InsertEventCompletedEventArgs>> InsertEVCompleted;
        IObservable<EventPattern<InsertSpecimenCompletedEventArgs>> InsertSPCompleted;
        IObservable<EventPattern<InsertIdentificationUnitCompletedEventArgs>> InsertIUCompleted;

        //DOWNLOAD
        IObservable<EventPattern<EventSeriesByIDCompletedEventArgs>> EventSeriesByIDCompleted;
        IObservable<EventPattern<LocalizationsForSeriesCompletedEventArgs>> LocalizationsForSeriesCompleted;
        IObservable<EventPattern<EventsByLocalityCompletedEventArgs>> EventsByLocalityCompleted;
        IObservable<EventPattern<PropertiesForEventCompletedEventArgs>> PropertiesForEventCompleted;
        IObservable<EventPattern<SpecimenForEventCompletedEventArgs>> SpecimenForEventCompleted;
        IObservable<EventPattern<UnitsForSpecimenCompletedEventArgs>> UnitsForSpecimenCompleted;
        IObservable<EventPattern<SubUnitsForIUCompletedEventArgs>> SubUnitsForIUCompleted;
        IObservable<EventPattern<AnalysesForIUCompletedEventArgs>> AnalysesForIUCompleted;

        //MULTIMEDIA
        IObservable<EventPattern<MultimediaService.SubmitCompletedEventArgs>> UploadMultimediaCompleted;


        readonly IKeyMappingService Mapping;
        readonly ICredentialsService Credentials;

        private Client.UserCredentials _CurrentCredentials;
        private Client.UserCredentials GetCreds() { return _CurrentCredentials; }

        public DiversityServiceClient(ICredentialsService Credentials, IKeyMappingService Mapping)
        {            
            this.Mapping = Mapping;
            this.Credentials = Credentials;

            Credentials.CurrentCredentials().Where(c => c != null).Subscribe(c => _CurrentCredentials = c);

            GetUserInfoCompleted = Observable.FromEventPattern<GetUserInfoCompletedEventArgs>(h => _svc.GetUserInfoCompleted += h, h => _svc.GetUserInfoCompleted -= h);
            GetRepositoriesCompleted = Observable.FromEventPattern<GetRepositoriesCompletedEventArgs>(h => _svc.GetRepositoriesCompleted += h, h => _svc.GetRepositoriesCompleted -= h);
            GetPropertiesForUserCompleted = Observable.FromEventPattern<GetPropertiesForUserCompletedEventArgs>(h => _svc.GetPropertiesForUserCompleted += h, h => _svc.GetPropertiesForUserCompleted -= h);
            GetProjectsForUserCompleted = Observable.FromEventPattern<GetProjectsForUserCompletedEventArgs>(h => _svc.GetProjectsForUserCompleted += h, h => _svc.GetProjectsForUserCompleted -= h);
            GetStandardVocabularyCompleted = Observable.FromEventPattern<GetStandardVocabularyCompletedEventArgs>(d => _svc.GetStandardVocabularyCompleted += d, d => _svc.GetStandardVocabularyCompleted -= d);
            GetAnalysesForProjectCompleted = Observable.FromEventPattern<GetAnalysesForProjectCompletedEventArgs>(d => _svc.GetAnalysesForProjectCompleted += d, d => _svc.GetAnalysesForProjectCompleted -= d);
            GetAnalysisResultsForProjectCompleted = Observable.FromEventPattern<GetAnalysisResultsForProjectCompletedEventArgs>( d => _svc.GetAnalysisResultsForProjectCompleted += d, d => _svc.GetAnalysisResultsForProjectCompleted -= d);
            GetAnalysisTaxonomicGroupsForProjectCompleted = Observable.FromEventPattern<GetAnalysisTaxonomicGroupsForProjectCompletedEventArgs>(d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted += d, d => _svc.GetAnalysisTaxonomicGroupsForProjectCompleted -= d);

            GetTaxonListsForUser = Observable.FromEventPattern<GetTaxonListsForUserCompletedEventArgs>( d => _svc.GetTaxonListsForUserCompleted += d, d => _svc.GetTaxonListsForUserCompleted -= d);
            DownloadTaxonList = Observable.FromEventPattern<DownloadTaxonListCompletedEventArgs>( d => _svc.DownloadTaxonListCompleted += d, d => _svc.DownloadTaxonListCompleted -= d);
            GetQualificationsCompleted = Observable.FromEventPattern<GetQualificationsCompletedEventArgs>( d => _svc.GetQualificationsCompleted += d, d => _svc.GetQualificationsCompleted -= d);

            InsertMMOCompleted = Observable.FromEventPattern<AsyncCompletedEventArgs>(h => _svc.InsertMMOCompleted += h, h => _svc.InsertMMOCompleted -= h);
            InsertESCompleted = Observable.FromEventPattern<InsertEventSeriesCompletedEventArgs>(h => _svc.InsertEventSeriesCompleted += h, h => _svc.InsertEventSeriesCompleted -= h);
            InsertEVCompleted = Observable.FromEventPattern<InsertEventCompletedEventArgs>(h => _svc.InsertEventCompleted += h, h => _svc.InsertEventCompleted -= h);
            InsertSPCompleted = Observable.FromEventPattern<InsertSpecimenCompletedEventArgs>(h => _svc.InsertSpecimenCompleted += h, h => _svc.InsertSpecimenCompleted -= h);
            InsertIUCompleted = Observable.FromEventPattern<InsertIdentificationUnitCompletedEventArgs>(h => _svc.InsertIdentificationUnitCompleted += h, h => _svc.InsertIdentificationUnitCompleted -= h);

            UploadMultimediaCompleted = Observable.FromEventPattern<SubmitCompletedEventArgs>(h => _multimedia.SubmitCompleted += h, h => _multimedia.SubmitCompleted -= h);

            EventSeriesByIDCompleted = Observable.FromEventPattern<EventSeriesByIDCompletedEventArgs>(h => _svc.EventSeriesByIDCompleted += h, h => _svc.EventSeriesByIDCompleted -= h);
            LocalizationsForSeriesCompleted = Observable.FromEventPattern<LocalizationsForSeriesCompletedEventArgs>(h => _svc.LocalizationsForSeriesCompleted += h, h => _svc.LocalizationsForSeriesCompleted -= h);
            EventsByLocalityCompleted = Observable.FromEventPattern<EventsByLocalityCompletedEventArgs>(h => _svc.EventsByLocalityCompleted += h, h => _svc.EventsByLocalityCompleted -= h);
            PropertiesForEventCompleted = Observable.FromEventPattern<PropertiesForEventCompletedEventArgs>(h => _svc.PropertiesForEventCompleted += h, h => _svc.PropertiesForEventCompleted -= h);
            SpecimenForEventCompleted = Observable.FromEventPattern<SpecimenForEventCompletedEventArgs>(h => _svc.SpecimenForEventCompleted += h, h => _svc.SpecimenForEventCompleted -= h);
            UnitsForSpecimenCompleted = Observable.FromEventPattern<UnitsForSpecimenCompletedEventArgs>(h => _svc.UnitsForSpecimenCompleted += h, h => _svc.UnitsForSpecimenCompleted -= h);
            SubUnitsForIUCompleted = Observable.FromEventPattern<SubUnitsForIUCompletedEventArgs>(h => _svc.SubUnitsForIUCompleted += h, h => _svc.SubUnitsForIUCompleted -= h);
            AnalysesForIUCompleted = Observable.FromEventPattern<AnalysesForIUCompletedEventArgs>(h => _svc.AnalysesForIUCompleted += h, h => _svc.AnalysesForIUCompleted -= h);
        }
    }
}
