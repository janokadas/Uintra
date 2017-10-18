﻿import initCore from './../App_Plugins/Core/Content/Scripts/Core';
import browserCompatibility from './../App_Plugins/Core/BrowserCompatibility/BrowserCompatibility';
import centralFeed from './../App_Plugins/CentralFeed/centralFeed';
import initSearch from './../App_Plugins/Search/search';
import initActionLinkWithConfirm from "../App_Plugins/Core/Content/scripts/ActionLinkWithConfirm";
import initUsers from './../App_Plugins/Users/users';
import initNotification from './../App_Plugins/Notification/notification';
import initTags from './../App_Plugins/Tagging/tags';
import subscribe from "./../App_Plugins/Subscribe/subscribe";
import initNavigation from './../App_Plugins/Navigation/navigation';
import comment from "./../App_Plugins/Comments/comment";
import initBulletings from './../App_Plugins/Bulletins/bulletins';
import contentPanel from './../App_Plugins/Panels/ContentPanel/contentPanel';
import {} from './../App_Plugins/Panels/DocumentLibraryPanel/documentLibraryPanel';
import initEvents from './../App_Plugins/Events/events';
import news from './../App_Plugins/News/news';
import {} from './../App_Plugins/Likes/likes';
import confirmOnBeforeUnload from './../App_Plugins/Core/Content/Scripts/ConfirmOnBeforeUnload';
import initGroups from './../App_Plugins/Groups/groups';
import latestActivities from './../App_Plugins/LatestActivities/latestActivities';
import initFaqPanel from "./../App_Plugins/Panels/FaqPanel/faqPanel";

initCore();
centralFeed.init();
initSearch();
initActionLinkWithConfirm();
initUsers();
initNotification();
initTags();
subscribe.initOnLoad();
initNavigation();
initEvents();
comment.init();
contentPanel.init();
news.init();
initBulletings();
confirmOnBeforeUnload();
browserCompatibility.init();
initGroups();
initFaqPanel();
latestActivities.init();