import React from "react";
import "./Input.css";

const Input = ({ field, form, type}) => {
  return (
      <div>
    <label className={"Input-label"} htmlFor={field.name}>
      {field.name}: <input className={"Input"} {...field} type={type} />
  </label>
      </div>


  );
};
export default Input;
