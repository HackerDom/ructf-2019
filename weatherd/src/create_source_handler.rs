extern crate futures;
extern crate gotham;
extern crate hyper;
extern crate mime;

use std::sync::{Arc, Mutex};

use gotham::error::Result;
use gotham::handler::{Handler, HandlerFuture, NewHandler};
use gotham::state::State;

use gotham::helpers::http::response::*;

use hyper::{StatusCode, Error, Body, Response};
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


use tokio_core::reactor::Core;
use futures::{Stream};

use hyper::{Method};

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
        let time_string = time.timestamp();

        let is_public = query_param.is_public == "true";

        let query_string = format!(
//            "source={}&time={}&isPublic={}",
            "source={}&time={}&password={}",
            query_param.name,
            time_string,
            query_param.password
//            is_public.to_string()
        );

        println!("query_string, create source handler : {}", query_string);

        let uri = format!("{}addUserInfo?{}", crate::constants::NOTIFICATION_API_ADDR, query_string).parse::<Uri>().unwrap();

        let client = Client::new();



        let req = request::Builder::new()
//            .method("POST")
            .method("GET")
            .uri(uri)
            .header("Content-Length",0)
            .body(hyper::Body::empty())

            .unwrap();

        let mut result = client
            .request(req)
            .map_err(|e| println!("everything is bad"))

//        .map(|mut res|
            .then(|mut res|
                {
                    match res
                        {
                            Ok(response) => {
                                println!("everything is good");

//                                let token = response.body().map_err(|_| ()).fold(vec![], |mut acc, chunk| {
//                                    acc.extend_from_slice(&chunk);
//                                    Ok(acc)
//                                }).and_then(|v| String::from_utf8(v).map_err(|_| ())).wait().unwrap();
//
//                                let new_source = WeatherSource{name : query_param.name.to_string(), password : query_param.password.to_string(), token : token.to_string()};
//
//                                {
//                                    let mut v = self.weather_state.lock().unwrap();
//                                    v.add_source(&query_param.name,new_source);
//                                }

                                return future::ok((state, response));
                            }
                            Err(e) => {
//                                println!("something is wrong {}", e);
                                println!("something is wrong");
                                let response = create_response(&state, http::StatusCode::INTERNAL_SERVER_ERROR, mime::TEXT_PLAIN, "Something went completely wrong.");
                                return future::ok((state, response));
                            }
                        }
                })

        ;
        println!("sended something create source handled");


        return Box::new(result);
    }
}

impl NewHandler for CreateSourceHandler {
    type Instance = Self;

    fn new_handler(&self) -> Result<Self::Instance> {
        Ok(self.clone())
    }
}
