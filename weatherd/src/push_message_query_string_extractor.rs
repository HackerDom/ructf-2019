use gotham::router::builder::*;
use gotham::router::Router;
use gotham::state::{FromState, State};
use gotham::router::response::extender;

//#[derive(Deserialize, StateData, StaticResponseExtender)]
//pub struct PushMessageQueryStringExtractor {
//    pub name: String,
//    pub message: String,
//    pub token: String
//}
