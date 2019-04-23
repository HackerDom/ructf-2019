use gotham::router::builder::*;
use gotham::router::Router;
use gotham::state::{FromState, State};
use gotham::router::response::extender;

#[derive(Deserialize, StateData, StaticResponseExtender)]
pub struct CreateSourceQueryStringExtractor {
    pub name: String,
    pub password : String,
    pub is_public : String,
}
