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
use crate::create_source_query_string_extractor::PushMessageQueryStringExtractor;

use crate::weather_state::WeatherSource;
use crate::weather_state::WeatherState;
use chrono::{DateTime, Utc};


#[derive(Clone)]
pub struct CreateSourceHandler {
    weather_state: Arc<Mutex<WeatherState>>,
}

impl CreateSourceHandler {
    pub fn new(state : &Arc<Mutex<WeatherState>>) -> CreateSourceHandler {
        CreateSourceHandler {
            weather_state: state.clone()
        }
    }
}

impl Handler for CreateSourceHandler {
    fn handle(self, mut state: State) -> Box<HandlerFuture> {

        let query_param = CreateSourceQueryStringExtractor::take_from(&mut state);

        let time = Utc::now();
        let time_string = time.to_rfc3339();

        let query_string = format!(
            "source={}&time={}",
            query_param.name,
            time_string
        );

        let uri = format!("http://localhost:5000/addUserInfo?{}", query_string).parse::<Uri>().unwrap();

        let client = Client::new();

        let new_source = WeatherSource{name : query_param.name.to_string(), password : query_param.password.to_string()};

        {
            let mut v = self.weather_state.lock().unwrap();
            v.add_source(&query_param.name,new_source);
        }

        let req = request::Builder::new()
            .method("POST")
            .uri(uri)
            .body(hyper::Body::empty())
            .unwrap();

        let result = client
            .request(req)
            .then(|res|
                {
                    match res
                        {
                            Ok(response) => {
                                if response.status() == hyper::StatusCode::OK
                                {
                                    let response = create_response(&state, StatusCode::OK, mime::TEXT_PLAIN, "Response is OK.");
                                    return future::ok((state, response));
                                }
                                let response = create_response(&state, http::StatusCode::INTERNAL_SERVER_ERROR, mime::TEXT_PLAIN, "Something went wrong.");
                                return future::ok((state, response));
                            }
                            Err(e) => {
                                println!("something is wrong {}", e);
                                let response = create_response(&state, http::StatusCode::INTERNAL_SERVER_ERROR, mime::TEXT_PLAIN, "Something went completely wrong.");
                                return future::ok((state, response));
                            }
                        }
                });

        return Box::new(result);
    }
}

impl NewHandler for CreateSourceHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}