extern crate futures;
extern crate gotham;
extern crate hyper;
extern crate mime;

use std::sync::{Arc, Mutex};

use gotham::error::Result;
use gotham::handler::{Handler, HandlerFuture, NewHandler};
use gotham::state::State;

use gotham::helpers::http::response::*;

use hyper::StatusCode;
use hyper::Client;

use http::{Uri, request};

use futures::future;
use futures::future::Future;

use gotham::state::{FromState};

use crate::create_source_query_string_extractor::CreateSourceQueryStringExtractor;
use crate::push_message_query_string_extractor::PushMessageQueryStringExtractor;

use crate::weather_state::WeatherSource;
use crate::weather_state::WeatherState;
use chrono::{DateTime, Utc};


#[derive(Clone)]
pub struct GetListOfSourcesHandler {
    weather_state: Arc<Mutex<WeatherState>>,
}

impl GetListOfSourcesHandler {
    pub fn new(state : &Arc<Mutex<WeatherState>>) -> GetListOfSourcesHandler {
        GetListOfSourcesHandler {
            weather_state: state.clone()
        }
    }
}

impl Handler for GetListOfSourcesHandler {
    fn handle(self, mut state: State) -> Box<HandlerFuture> {

        let sources : String;

        {
            let mut v = self.weather_state.lock().unwrap();
            sources = v.get_all_sources().iter().map(|(k ,v)| k.to_string()).collect::<Vec<String>>().connect("\n");
        }

        let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, sources);
        let result = future::ok((state, response));

        return Box::new(result);
    }
}

impl NewHandler for GetListOfSourcesHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}