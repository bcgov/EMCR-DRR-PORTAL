using AutoMapper;
using EMCR.DRR.API.Resources.Cases;
using EMCR.DRR.API.Services;
using EMCR.DRR.Resources.Applications;

namespace EMCR.DRR.Managers.Intake
{
    public class IntakeManager : IIntakeManager
    {
        private readonly IMapper mapper;
        private readonly IApplicationRepository applicationRepository;
        private readonly ICaseRepository caseRepository;

        public IntakeManager(IMapper mapper, IApplicationRepository applicationRepository, ICaseRepository caseRepository)
        {
            this.mapper = mapper;
            this.applicationRepository = applicationRepository;
            this.caseRepository = caseRepository;
        }

        public async Task<IntakeQueryResponse> Handle(IntakeQuery cmd)
        {
            return cmd switch
            {
                DrrApplicationsQuery c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<string> Handle(IntakeCommand cmd)
        {
            return cmd switch
            {
                EoiSaveApplicationCommand c => await Handle(c),
                EoiSubmitApplicationCommand c => await Handle(c),
                CreateFpFromEoiCommand c => await Handle(c),
                FpSaveApplicationCommand c => await Handle(c),
                FpSubmitApplicationCommand c => await Handle(c),
                _ => throw new NotSupportedException($"{cmd.GetType().Name} is not supported")
            };
        }

        public async Task<IntakeQueryResponse> Handle(DrrApplicationsQuery q)
        {
            if (!string.IsNullOrEmpty(q.Id))
            {
                var canAccess = await CanAccessApplication(q.Id, q.BusinessId);
                if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            }
            var res = await applicationRepository.Query(new ApplicationsQuery { Id = q.Id, BusinessId = q.BusinessId });
            return new IntakeQueryResponse { Items = mapper.Map<IEnumerable<Application>>(res.Items) };
        }

        public async Task<string> Handle(EoiSaveApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SubmitApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(EoiSubmitApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            application.SubmittedDate = DateTime.UtcNow;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SubmitApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(CreateFpFromEoiCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.EoiId, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");

            var res = (await caseRepository.Manage(new GenerateFpFromEoi { EoiId = cmd.EoiId })).Id;
            return res;
        }

        public async Task<string> Handle(FpSaveApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SubmitApplication { Application = application })).Id;
            return id;
        }

        public async Task<string> Handle(FpSubmitApplicationCommand cmd)
        {
            var canAccess = await CanAccessApplication(cmd.application.Id, cmd.UserInfo.BusinessId);
            if (!canAccess) throw new ForbiddenException("Not allowed to access this application.");
            var application = mapper.Map<Application>(cmd.application);
            application.BCeIDBusinessId = cmd.UserInfo.BusinessId;
            application.ProponentName = cmd.UserInfo.BusinessName;
            application.SubmittedDate = DateTime.UtcNow;
            if (application.Submitter != null) application.Submitter.BCeId = cmd.UserInfo.UserId;
            var id = (await applicationRepository.Manage(new SubmitApplication { Application = application })).Id;
            return id;
        }

        public async Task<DeclarationQueryResult> Handle(DeclarationQuery _)
        {
            var res = await applicationRepository.Query(new Resources.Applications.DeclarationQuery());
            return new DeclarationQueryResult { Items = mapper.Map<IEnumerable<DeclarationInfo>>(res.Items) };
        }

        public async Task<EntitiesQueryResult> Handle(EntitiesQuery _)
        {
            var res = await applicationRepository.Query(new Resources.Applications.EntitiesQuery());
            return mapper.Map<EntitiesQueryResult>(res);
        }

        private async Task<bool> CanAccessApplication(string? id, string? businessId)
        {
            if (string.IsNullOrEmpty(businessId)) throw new ArgumentNullException("Missing user's BusinessId");
            if (string.IsNullOrEmpty(id)) return true;
            return await applicationRepository.CanAccessApplication(id, businessId);
        }
    }
}
