using log4net;
using NHibernate.Event;
using NHibernate.Event.Default;

namespace NHibernate.CollectionTest
{
    public class OnLoadEventListener : DefaultPostLoadEventListener {
        private readonly ILog _logger;

        public OnLoadEventListener() {
            _logger = LogManager.GetLogger(typeof(OnLoadEventListener));
        }

        public override void OnPostLoad(PostLoadEvent theEvent) {
            base.OnPostLoad(theEvent);

            ISelfTrackingEntity entity = theEvent.Entity as ISelfTrackingEntity;
            if (entity != null) {
                entity.OnPostLoad();
            }
        }
    }

    public interface ISelfTrackingEntity
    {
        void OnPostLoad();
    }
}
